namespace TwelveRanks.Core.Engine;

internal static class InitialPosition
{
    private static readonly PieceKind?[] BackRank =
    [
        PieceKind.Rook,
        PieceKind.Chancellor,
        PieceKind.Knight,
        PieceKind.Bishop,
        PieceKind.Minister,
        PieceKind.Queen,
        PieceKind.King,
        PieceKind.Minister,
        PieceKind.Bishop,
        PieceKind.Knight,
        PieceKind.Chancellor,
        PieceKind.Rook
    ];

    public static IEnumerable<PiecePlacement> Create()
    {
        foreach (PiecePlacement placement in CreateSide(
                     PieceSide.Black,
                     0,
                     1))
        {
            yield return placement;
        }

        foreach (PiecePlacement placement in CreateSide(
                     PieceSide.White,
                     BoardSpecification.Ranks - 1,
                     BoardSpecification.Ranks - 2))
        {
            yield return placement;
        }
    }

    private static IEnumerable<PiecePlacement> CreateSide(
        PieceSide side,
        int backRank,
        int pawnRank)
    {
        int queenFile = Array.IndexOf(BackRank, PieceKind.Queen);
        int kingFile = Array.IndexOf(BackRank, PieceKind.King);

        for (int file = 0; file < BackRank.Length; file++)
        {
            if (BackRank[file] is PieceKind pieceKind)
            {
                yield return new PiecePlacement(
                    new BoardSquare(backRank, file),
                    new BoardPiece(pieceKind, side));
            }

            PawnRole pawnRole = GetPawnRole(file, queenFile, kingFile);

            yield return new PiecePlacement(
                new BoardSquare(pawnRank, file),
                new BoardPiece(PieceKind.Pawn, side, pawnRole));
        }
    }

    private static PawnRole GetPawnRole(
        int file,
        int queenFile,
        int kingFile)
    {
        if (file == queenFile)
        {
            return PawnRole.QueenPawn;
        }

        return file == kingFile
            ? PawnRole.KingPawn
            : PawnRole.Regular;
    }
}
