using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwelveRanks.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace TwelveRanks.App.Services;

internal enum MoveSoundCue
{
    Move,
    Capture,
    Castle,
    Promotion,
    Check,
    GameOver
}

internal static class MoveSoundService
{
    private const int SampleRate = 22050;
    private const short ChannelCount = 1;
    private const short BitsPerSample = 16;

    private static readonly SemaphoreSlim PlaybackGate = new(1, 1);
    private static readonly MediaPlayer Player = CreatePlayer();
    private static readonly byte[] MoveWave =
        CreateWave(
            new Tone(235, 55, 0.28),
            new Tone(0, 12, 0),
            new Tone(310, 45, 0.22));
    private static readonly byte[] CaptureWave =
        CreateWave(
            new Tone(185, 75, 0.42),
            new Tone(118, 95, 0.36));
    private static readonly byte[] CastleWave =
        CreateWave(
            new Tone(220, 50, 0.26),
            new Tone(277, 50, 0.25),
            new Tone(330, 90, 0.24));
    private static readonly byte[] PromotionWave =
        CreateWave(
            new Tone(392, 65, 0.24),
            new Tone(523, 70, 0.27),
            new Tone(659, 125, 0.30));
    private static readonly byte[] CheckWave =
        CreateWave(
            new Tone(740, 85, 0.25),
            new Tone(0, 35, 0),
            new Tone(740, 110, 0.28));
    private static readonly byte[] GameOverWave =
        CreateWave(
            new Tone(523, 90, 0.27),
            new Tone(392, 110, 0.26),
            new Tone(262, 180, 0.30));

    private static IRandomAccessStream? _currentStream;

    internal static void Play(MoveResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!AppSettingsService.PlayMoveSounds)
        {
            return;
        }

        _ = PlayAsync(SelectCue(result));
    }

    internal static MoveSoundCue SelectCue(MoveResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Status is not GameStatus.Active and not GameStatus.Check)
        {
            return MoveSoundCue.GameOver;
        }

        if (result.Move.IsPromotion)
        {
            return MoveSoundCue.Promotion;
        }

        if (result.CapturedPiece.HasValue)
        {
            return MoveSoundCue.Capture;
        }

        if (result.Move.Kind is
            MoveKind.CastleTowardLowerFile or
            MoveKind.CastleTowardHigherFile)
        {
            return MoveSoundCue.Castle;
        }

        return result.Status == GameStatus.Check
            ? MoveSoundCue.Check
            : MoveSoundCue.Move;
    }

    private static MediaPlayer CreatePlayer()
    {
        MediaPlayer player = new()
        {
            AudioCategory = MediaPlayerAudioCategory.GameMedia,
            AutoPlay = false,
            Volume = 0.65
        };
        player.CommandManager.IsEnabled = false;
        return player;
    }

    private static async Task PlayAsync(MoveSoundCue cue)
    {
        await PlaybackGate.WaitAsync().ConfigureAwait(true);

        try
        {
            InMemoryRandomAccessStream stream = new();

            try
            {
                using (DataWriter writer = new(stream))
                {
                    writer.WriteBytes(GetWave(cue));
                    _ = await writer.StoreAsync();
                    _ = writer.DetachStream();
                }

                stream.Seek(0);
                IRandomAccessStream? previousStream =
                    _currentStream;

                Player.Pause();
                Player.Source = null;
                _currentStream = stream;
                Player.Source = MediaSource.CreateFromStream(
                    stream,
                    "audio/wav");
                Player.Play();
                previousStream?.Dispose();
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }
        catch (ArgumentException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (COMException)
        {
        }
        finally
        {
            _ = PlaybackGate.Release();
        }
    }

    private static byte[] GetWave(MoveSoundCue cue)
    {
        return cue switch
        {
            MoveSoundCue.Move => MoveWave,
            MoveSoundCue.Capture => CaptureWave,
            MoveSoundCue.Castle => CastleWave,
            MoveSoundCue.Promotion => PromotionWave,
            MoveSoundCue.Check => CheckWave,
            MoveSoundCue.GameOver => GameOverWave,
            _ => throw new ArgumentOutOfRangeException(
                nameof(cue),
                cue,
                "Unknown move sound cue.")
        };
    }

    private static byte[] CreateWave(params Tone[] tones)
    {
        int sampleCount = 0;

        foreach (Tone tone in tones)
        {
            sampleCount += SampleRate * tone.Milliseconds / 1000;
        }

        int dataLength =
            sampleCount * ChannelCount * (BitsPerSample / 8);
        using MemoryStream stream = new(44 + dataLength);
        using BinaryWriter writer = new(
            stream,
            Encoding.ASCII,
            true);

        writer.Write("RIFF"u8);
        writer.Write(36 + dataLength);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(ChannelCount);
        writer.Write(SampleRate);
        writer.Write(
            SampleRate * ChannelCount * (BitsPerSample / 8));
        writer.Write(
            (short)(ChannelCount * (BitsPerSample / 8)));
        writer.Write(BitsPerSample);
        writer.Write("data"u8);
        writer.Write(dataLength);

        foreach (Tone tone in tones)
        {
            WriteTone(writer, tone);
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteTone(
        BinaryWriter writer,
        Tone tone)
    {
        int sampleCount =
            SampleRate * tone.Milliseconds / 1000;

        for (int index = 0; index < sampleCount; index++)
        {
            double progress = (double)index / sampleCount;
            double attack = Math.Min(
                1,
                index / (SampleRate * 0.004));
            double envelope =
                attack * Math.Pow(1 - progress, 2.4);
            double wave = tone.Frequency <= 0
                ? 0
                : Math.Sin(
                    2 *
                    Math.PI *
                    tone.Frequency *
                    index /
                    SampleRate);
            short sample = (short)(
                wave *
                envelope *
                tone.Volume *
                short.MaxValue);
            writer.Write(sample);
        }
    }

    private readonly record struct Tone(
        double Frequency,
        int Milliseconds,
        double Volume);
}
