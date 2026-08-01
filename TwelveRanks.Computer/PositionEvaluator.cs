using TwelveRanks.Core;

namespace TwelveRanks.Computer;

internal static class PositionEvaluator
{
    public const int Infinity = 1_100_000;
    public const int MateScore = 1_000_000;

    private const int BishopValue = 330;
    private const int CastlingRightValue = 10;
    private const int ChancellorValue = 820;
    private const int CheckPenalty = 25;
    private const int MinisterValue = 650;
    private const int KnightValue = 320;
    private const int PawnValue = 100;
    private const int QueenValue = 900;
    private const int RookValue = 500;
    private const int RoyalGuardPremium = 40;

    public static int Evaluate(ChessGame game)
    {
        int whiteScore = 0;

        foreach (PiecePlacement placement in game.GetPieces())
        {
            int pieceScore =
                GetMaterialValue(placement.Piece) +
                GetPositionValue(placement);

            whiteScore += placement.Piece.Side == PieceSide.White
                ? pieceScore
                : -pieceScore;
        }

        whiteScore += GetCastlingValue(
            game.CastlingRights,
            PieceSide.White);
        whiteScore -= GetCastlingValue(
            game.CastlingRights,
            PieceSide.Black);
        whiteScore += GetCheckAdjustment(game);

        return game.SideToMove == PieceSide.White
            ? whiteScore
            : -whiteScore;
    }

    public static int GetMaterialValue(BoardPiece piece)
    {
        int value = piece.Kind switch
        {
            PieceKind.King => 0,
            PieceKind.Queen => QueenValue,
            PieceKind.Rook => RookValue,
            PieceKind.Bishop => BishopValue,
            PieceKind.Knight => KnightValue,
            PieceKind.Pawn => PawnValue,
            PieceKind.Minister => MinisterValue,
            PieceKind.Chancellor => ChancellorValue,
            _ => throw new ArgumentOutOfRangeException(
                nameof(piece),
                piece,
                "The piece kind is not supported by the evaluator.")
        };

        return piece.PawnRole == PawnRole.Regular
            ? value
            : value + RoyalGuardPremium;
    }

    public static int GetTerminalScore(GameStatus status, int ply)
    {
        return status switch
        {
            GameStatus.Checkmate => -MateScore + ply,
            GameStatus.Stalemate => 0,
            GameStatus.DrawByThreefoldRepetition => 0,
            GameStatus.DrawByFiftyMoveRule => 0,
            GameStatus.DrawByInsufficientMaterial => 0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The supplied game status is not terminal.")
        };
    }

    private static int GetPositionValue(PiecePlacement placement)
    {
        int centerBonus = GetCenterBonus(placement.Square);

        return placement.Piece.Kind switch
        {
            PieceKind.King => 0,
            PieceKind.Queen => centerBonus,
            PieceKind.Rook => centerBonus,
            PieceKind.Bishop => centerBonus * 2,
            PieceKind.Knight => centerBonus * 4,
            PieceKind.Pawn =>
                GetPawnAdvancement(placement) * 4 + centerBonus,
            PieceKind.Minister => centerBonus * 3,
            PieceKind.Chancellor => centerBonus * 2,
            _ => 0
        };
    }

    private static int GetCenterBonus(BoardSquare square)
    {
        int doubledFileDistance = Math.Abs(
            (square.File * 2) - (BoardSpecification.Files - 1));
        int doubledRankDistance = Math.Abs(
            (square.Rank * 2) - (BoardSpecification.Ranks - 1));
        int maximumDistance =
            (BoardSpecification.Files - 1) +
            (BoardSpecification.Ranks - 1);

        return (maximumDistance -
            doubledFileDistance -
            doubledRankDistance) / 2;
    }

    private static int GetPawnAdvancement(PiecePlacement placement)
    {
        return placement.Piece.Side == PieceSide.White
            ? (BoardSpecification.Ranks - 1) - placement.Square.Rank
            : placement.Square.Rank;
    }

    private static int GetCastlingValue(
        CastlingRights rights,
        PieceSide side)
    {
        CastlingRights lowerFileRight = side == PieceSide.White
            ? CastlingRights.WhiteTowardLowerFile
            : CastlingRights.BlackTowardLowerFile;
        CastlingRights higherFileRight = side == PieceSide.White
            ? CastlingRights.WhiteTowardHigherFile
            : CastlingRights.BlackTowardHigherFile;

        int value = HasRight(rights, lowerFileRight)
            ? CastlingRightValue
            : 0;

        return HasRight(rights, higherFileRight)
            ? value + CastlingRightValue
            : value;
    }

    private static bool HasRight(
        CastlingRights rights,
        CastlingRights right)
    {
        return (rights & right) != CastlingRights.None;
    }

    private static int GetCheckAdjustment(ChessGame game)
    {
        if (game.Status != GameStatus.Check)
        {
            return 0;
        }

        return game.SideToMove == PieceSide.White
            ? -CheckPenalty
            : CheckPenalty;
    }
}
