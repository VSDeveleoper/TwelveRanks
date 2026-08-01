using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TwelveRanks.Core;
using Windows.Storage;

namespace TwelveRanks.App.Services;

internal static partial class GameSaveService
{
    private const string SaveFileName = "unfinished-game.json";
    private const int SaveFormatVersion = 1;

    private static readonly SemaphoreSlim FileGate = new(1, 1);
    private static string SaveFilePath =>
        Path.Combine(
            ApplicationData.Current.LocalFolder.Path,
            SaveFileName);

    private static string TemporarySaveFilePath =>
        string.Concat(SaveFilePath, ".tmp");

    internal static async Task<SavedGame?> TryLoadAsync()
    {
        await FileGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                return null;
            }

            string json = await File.ReadAllTextAsync(
                SaveFilePath).ConfigureAwait(false);
            SavedGameFile? savedFile =
                JsonSerializer.Deserialize(
                    json,
                    GameSaveJsonContext.Default.SavedGameFile);

            return TryReplay(savedFile, out SavedGame? savedGame)
                ? savedGame
                : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        finally
        {
            _ = FileGate.Release();
        }
    }

    internal static async Task<bool> SaveOrClearAsync(
        ChessGame game,
        GameMode mode)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (!IsSupportedMode(mode) ||
            game.IsGameOver ||
            game.MoveHistory.Count == 0)
        {
            return await ClearAsync().ConfigureAwait(false);
        }

        SavedGameFile savedFile = new(
            SaveFormatVersion,
            mode,
            [.. game.MoveHistory.Select(
                static result => SavedMove.FromMove(result.Move))]);
        string json = JsonSerializer.Serialize(
            savedFile,
            GameSaveJsonContext.Default.SavedGameFile);

        await FileGate.WaitAsync().ConfigureAwait(false);
        try
        {
            await File.WriteAllTextAsync(
                TemporarySaveFilePath,
                json).ConfigureAwait(false);
            File.Move(
                TemporarySaveFilePath,
                SaveFilePath,
                true);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            _ = FileGate.Release();
        }
    }

    internal static async Task<bool> ClearAsync()
    {
        await FileGate.WaitAsync().ConfigureAwait(false);
        try
        {
            File.Delete(SaveFilePath);
            File.Delete(TemporarySaveFilePath);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            _ = FileGate.Release();
        }
    }

    private static bool TryReplay(
        SavedGameFile? savedFile,
        out SavedGame? savedGame)
    {
        savedGame = null;

        if (savedFile is null ||
            savedFile.Version != SaveFormatVersion ||
            !IsSupportedMode(savedFile.Mode) ||
            savedFile.Moves is not { Count: > 0 })
        {
            return false;
        }

        ChessGame game = ChessGame.CreateInitial();

        try
        {
            foreach (SavedMove savedMove in savedFile.Moves)
            {
                ChessMove move = savedMove.ToMove();

                if (!game.TryMakeMove(move, out _))
                {
                    return false;
                }
            }
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (game.IsGameOver)
        {
            return false;
        }

        savedGame = new SavedGame(savedFile.Mode, game);
        return true;
    }

    private static bool IsSupportedMode(GameMode mode)
    {
        return mode is GameMode.LocalTwoPlayer or GameMode.Computer;
    }

    private sealed record SavedGameFile(
        int Version,
        GameMode Mode,
        List<SavedMove> Moves);

    private readonly record struct SavedMove(
        int FromRank,
        int FromFile,
        int ToRank,
        int ToFile,
        MoveKind Kind,
        PieceKind? PromotionPiece)
    {
        internal static SavedMove FromMove(ChessMove move)
        {
            return new SavedMove(
                move.From.Rank,
                move.From.File,
                move.To.Rank,
                move.To.File,
                move.Kind,
                move.PromotionPiece);
        }

        internal ChessMove ToMove()
        {
            return new ChessMove(
                new BoardSquare(FromRank, FromFile),
                new BoardSquare(ToRank, ToFile),
                Kind,
                PromotionPiece);
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(SavedGameFile))]
    private sealed partial class GameSaveJsonContext : JsonSerializerContext
    {
    }
}

internal sealed record SavedGame(GameMode Mode, ChessGame Game);
