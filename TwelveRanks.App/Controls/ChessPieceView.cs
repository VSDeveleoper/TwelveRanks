using TwelveRanks.Core;
using TwelveRanks.App.Services;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TwelveRanks.App.Controls;

internal sealed partial class ChessPieceView : Grid
{
    private static readonly SolidColorBrush IndicatorBrush =
        new(ColorHelper.FromArgb(255, 211, 47, 47));

    private static readonly SolidColorBrush IndicatorOutlineBrush =
        new(ColorHelper.FromArgb(255, 255, 255, 255));

    public ChessPieceView(BoardPiece piece)
    {
        string pieceName = ChessText.GetAssetSlug(piece.Kind);
        Uri source = new(
            $"ms-appx:///Assets/Pieces/Source/{pieceName}.svg");

        Image pieceImage = new()
        {
            Stretch = Stretch.Uniform,
            Margin = new Thickness(5),
            IsHitTestVisible = false
        };
        Children.Add(pieceImage);

        bool artworkResolved = false;
        Loaded += async (_, _) =>
        {
            if (artworkResolved)
            {
                return;
            }

            try
            {
                await LoadArtworkAsync(pieceImage, source, piece.Side)
                    .ConfigureAwait(true);
            }
            catch (IOException)
            {
                ShowArtworkFallback(pieceImage, piece);
            }
            catch (UnauthorizedAccessException)
            {
                ShowArtworkFallback(pieceImage, piece);
            }
            catch (COMException)
            {
                ShowArtworkFallback(pieceImage, piece);
            }
            catch (ArgumentException)
            {
                ShowArtworkFallback(pieceImage, piece);
            }
            catch (InvalidOperationException)
            {
                ShowArtworkFallback(pieceImage, piece);
            }

            artworkResolved = true;
        };

        if (piece.PawnRole != PawnRole.Regular)
        {
            Children.Add(new Border
            {
                Width = 13,
                Height = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 0, 0),
                Background = IndicatorBrush,
                BorderBrush = IndicatorOutlineBrush,
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(6.5)
            });
        }

        string pieceDescription = ChessText.GetPieceDescription(piece);
        AutomationProperties.SetName(this, pieceDescription);
        ToolTipService.SetToolTip(this, pieceDescription);
    }

    private void ShowArtworkFallback(Image image, BoardPiece piece)
    {
        image.Visibility = Visibility.Collapsed;
        Viewbox fallback = new()
        {
            Margin = new Thickness(5),
            Stretch = Stretch.Uniform,
            Child = new TextBlock
            {
                Text = GetFallbackSymbol(piece),
                FontFamily = new FontFamily("Segoe UI Symbol"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        Children.Insert(0, fallback);
    }

    private static async System.Threading.Tasks.Task LoadArtworkAsync(
        Image image,
        Uri source,
        PieceSide side)
    {
        StorageFile sourceFile =
            await StorageFile.GetFileFromApplicationUriAsync(source);
        string svg = await FileIO.ReadTextAsync(sourceFile);
        string fill = side == PieceSide.White ? "#fff8e7" : "#241f1b";
        string outline = side == PieceSide.White ? "#3c2e25" : "#ecd8ae";
        string coloredSvg = svg.Replace(
            "fill=\"currentColor\"",
            $"fill=\"{fill}\" stroke=\"{outline}\" " +
            "stroke-width=\"14\" stroke-linejoin=\"round\"",
            StringComparison.Ordinal);

        using InMemoryRandomAccessStream stream = new();
        using (DataWriter writer = new(stream))
        {
            writer.UnicodeEncoding = UnicodeEncoding.Utf8;
            _ = writer.WriteString(coloredSvg);
            _ = await writer.StoreAsync();
            _ = writer.DetachStream();
        }

        stream.Seek(0);
        SvgImageSource imageSource = new();
        _ = await imageSource.SetSourceAsync(stream);
        image.Source = imageSource;
    }

    private static string GetFallbackSymbol(BoardPiece piece)
    {
        return (piece.Side, piece.Kind) switch
        {
            (PieceSide.White, PieceKind.King) => "♔",
            (PieceSide.White, PieceKind.Queen) => "♕",
            (PieceSide.White, PieceKind.Rook) => "♖",
            (PieceSide.White, PieceKind.Bishop) => "♗",
            (PieceSide.White, PieceKind.Knight) => "♘",
            (PieceSide.White, PieceKind.Pawn) => "♙",
            (PieceSide.Black, PieceKind.King) => "♚",
            (PieceSide.Black, PieceKind.Queen) => "♛",
            (PieceSide.Black, PieceKind.Rook) => "♜",
            (PieceSide.Black, PieceKind.Bishop) => "♝",
            (PieceSide.Black, PieceKind.Knight) => "♞",
            (PieceSide.Black, PieceKind.Pawn) => "♟",
            (PieceSide.White, PieceKind.Minister) => "♗♘",
            (PieceSide.Black, PieceKind.Minister) => "♝♞",
            (PieceSide.White, PieceKind.Chancellor) => "♖♘",
            (PieceSide.Black, PieceKind.Chancellor) => "♜♞",
            _ => "?"
        };
    }
}
