using System;
using TwelveRanks.Core;

namespace TwelveRanks.App.Services;

internal static class ChessText
{
    internal static string GetPieceDescription(BoardPiece piece)
    {
        string pieceName = piece.PawnRole switch
        {
            PawnRole.QueenPawn or PawnRole.KingPawn =>
                LocalizationResources.GetString("PieceRoyalGuardPawn"),
            _ => GetPieceName(piece.Kind)
        };

        return LocalizationResources.Format(
            "PieceDescriptionFormat",
            GetSideName(piece.Side),
            pieceName);
    }

    internal static string GetSideName(PieceSide side)
    {
        return side switch
        {
            PieceSide.White =>
                LocalizationResources.GetString("SideWhite"),
            PieceSide.Black =>
                LocalizationResources.GetString("SideBlack"),
            _ => throw new ArgumentOutOfRangeException(
                nameof(side),
                side,
                "Unknown chess piece side.")
        };
    }

    internal static string GetPieceName(PieceKind pieceKind)
    {
        return pieceKind switch
        {
            PieceKind.King =>
                LocalizationResources.GetString("PieceKing"),
            PieceKind.Queen =>
                LocalizationResources.GetString("PieceQueen"),
            PieceKind.Rook =>
                LocalizationResources.GetString("PieceRook"),
            PieceKind.Bishop =>
                LocalizationResources.GetString("PieceBishop"),
            PieceKind.Knight =>
                LocalizationResources.GetString("PieceKnight"),
            PieceKind.Pawn =>
                LocalizationResources.GetString("PiecePawn"),
            PieceKind.Minister =>
                LocalizationResources.GetString("PieceMinister"),
            PieceKind.Chancellor =>
                LocalizationResources.GetString("PieceChancellor"),
            _ => throw new ArgumentOutOfRangeException(
                nameof(pieceKind),
                pieceKind,
                "Unknown chess piece kind.")
        };
    }

    internal static string GetPromotionPieceName(PieceKind pieceKind)
    {
        return pieceKind switch
        {
            PieceKind.Queen or
            PieceKind.Rook or
            PieceKind.Bishop or
            PieceKind.Knight or
            PieceKind.Chancellor => GetPieceName(pieceKind),
            PieceKind.Minister =>
                LocalizationResources.GetString(
                    "PromotionPieceArchbishop"),
            _ => throw new ArgumentOutOfRangeException(
                nameof(pieceKind),
                pieceKind,
                "This piece cannot be selected for promotion.")
        };
    }

    internal static string GetAssetSlug(PieceKind pieceKind)
    {
        return pieceKind switch
        {
            PieceKind.King => "king",
            PieceKind.Queen => "queen",
            PieceKind.Rook => "rook",
            PieceKind.Bishop => "bishop",
            PieceKind.Knight => "knight",
            PieceKind.Pawn => "pawn",
            PieceKind.Minister => "minister",
            PieceKind.Chancellor => "chancellor",
            _ => throw new ArgumentOutOfRangeException(
                nameof(pieceKind),
                pieceKind,
                "Unknown chess piece kind.")
        };
    }
}
