using TwelveRanks.Core;

namespace TwelveRanks.Tests;

internal static class TestGameFactory
{
    public static ChessGame Create(
        PieceSide sideToMove,
        CastlingRights castlingRights,
        params PiecePlacement[] additionalPieces)
    {
        List<PiecePlacement> placements =
        [
            Place(PieceKind.King, PieceSide.White, 11, 0),
            Place(PieceKind.King, PieceSide.Black, 0, 11)
        ];

        placements.AddRange(additionalPieces);
        return ChessGame.CreateCustom(
            placements,
            sideToMove,
            castlingRights);
    }

    public static ChessGame Create(
        params PiecePlacement[] additionalPieces)
    {
        return Create(
            PieceSide.White,
            CastlingRights.None,
            additionalPieces);
    }

    public static PiecePlacement Place(
        PieceKind kind,
        PieceSide side,
        int rank,
        int file,
        PawnRole pawnRole = PawnRole.Regular)
    {
        return new PiecePlacement(
            new BoardSquare(rank, file),
            new BoardPiece(kind, side, pawnRole));
    }

    public static ChessMove FindMove(
        ChessGame game,
        BoardSquare from,
        BoardSquare to,
        PieceKind? promotionPiece = null)
    {
        foreach (ChessMove move in game.GetLegalMoves(from))
        {
            if (move.To == to &&
                move.PromotionPiece == promotionPiece)
            {
                return move;
            }
        }

        throw new AssertFailedException(
            $"No legal move exists from {from} to {to}.");
    }

    public static HashSet<BoardSquare> Destinations(
        ChessGame game,
        BoardSquare from)
    {
        return [.. game.GetLegalMoves(from).Select(static move => move.To)];
    }
}
