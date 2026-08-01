namespace TwelveRanks.Core.Engine;

internal readonly record struct CastlingGeometry(
    CastlingRights RequiredRight,
    BoardSquare KingFrom,
    BoardSquare KingThrough,
    BoardSquare KingTo,
    BoardSquare RookFrom,
    BoardSquare RookTo)
{
    private const int KingStartingFile = 6;

    public static CastlingGeometry Get(
        PieceSide side,
        MoveKind moveKind)
    {
        int homeRank = side.HomeRank();

        return moveKind switch
        {
            MoveKind.CastleTowardLowerFile => new CastlingGeometry(
                side == PieceSide.White
                    ? CastlingRights.WhiteTowardLowerFile
                    : CastlingRights.BlackTowardLowerFile,
                new BoardSquare(homeRank, KingStartingFile),
                new BoardSquare(homeRank, 5),
                new BoardSquare(homeRank, 4),
                new BoardSquare(homeRank, 0),
                new BoardSquare(homeRank, 5)),
            MoveKind.CastleTowardHigherFile => new CastlingGeometry(
                side == PieceSide.White
                    ? CastlingRights.WhiteTowardHigherFile
                    : CastlingRights.BlackTowardHigherFile,
                new BoardSquare(homeRank, KingStartingFile),
                new BoardSquare(homeRank, 7),
                new BoardSquare(homeRank, 8),
                new BoardSquare(homeRank, BoardSpecification.Files - 1),
                new BoardSquare(homeRank, 7)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(moveKind),
                moveKind,
                "The move is not a castling move.")
        };
    }
}
