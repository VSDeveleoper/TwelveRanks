using TwelveRanks.Core;

namespace TwelveRanks.Computer;

internal static class MoveOrderer
{
    private const int CapturePriority = 100_000;
    private const int CastlingPriority = 2_000;
    private const int ExchangePriority = 1_000;
    private const int PreferredMovePriority = 1_000_000;
    private const int PromotionPriority = 200_000;

    public static List<ChessMove> Order(
        ChessGame game,
        IReadOnlyList<ChessMove> moves,
        ChessMove? preferredMove = null)
    {
        List<ChessMove> orderedMoves = [.. moves];

        orderedMoves.Sort((left, right) =>
            Compare(game, left, right, preferredMove));

        return orderedMoves;
    }

    public static bool IsTactical(ChessMove move)
    {
        return move.IsCapture || move.IsPromotion;
    }

    private static int Compare(
        ChessGame game,
        ChessMove left,
        ChessMove right,
        ChessMove? preferredMove)
    {
        int scoreComparison = GetScore(
            game,
            right,
            preferredMove).CompareTo(
                GetScore(game, left, preferredMove));

        return scoreComparison != 0
            ? scoreComparison
            : CompareCoordinates(left, right);
    }

    private static int CompareCoordinates(
        ChessMove left,
        ChessMove right)
    {
        int comparison = left.From.Index.CompareTo(right.From.Index);

        if (comparison == 0)
        {
            comparison = left.To.Index.CompareTo(right.To.Index);
        }

        if (comparison == 0)
        {
            comparison = left.Kind.CompareTo(right.Kind);
        }

        return comparison == 0
            ? GetPromotionOrdinal(left).CompareTo(
                GetPromotionOrdinal(right))
            : comparison;
    }

    private static int GetScore(
        ChessGame game,
        ChessMove move,
        ChessMove? preferredMove)
    {
        int score = preferredMove.HasValue &&
            move == preferredMove.Value
                ? PreferredMovePriority
                : 0;

        return score +
            GetPromotionScore(move) +
            GetCaptureScore(game, move) +
            GetSpecialMoveScore(move);
    }

    private static int GetPromotionScore(ChessMove move)
    {
        if (move.PromotionPiece is not PieceKind promotionKind)
        {
            return 0;
        }

        BoardPiece promotedPiece = new(
            promotionKind,
            PieceSide.White);

        return PromotionPriority +
            PositionEvaluator.GetMaterialValue(promotedPiece);
    }

    private static int GetCaptureScore(
        ChessGame game,
        ChessMove move)
    {
        if (!move.IsCapture ||
            GetCapturedPiece(game, move) is not BoardPiece capturedPiece)
        {
            return 0;
        }

        BoardPiece movingPiece = game.GetPiece(move.From)!.Value;

        return CapturePriority +
            (PositionEvaluator.GetMaterialValue(capturedPiece) * 16) -
            PositionEvaluator.GetMaterialValue(movingPiece);
    }

    private static BoardPiece? GetCapturedPiece(
        ChessGame game,
        ChessMove move)
    {
        if (move.Kind != MoveKind.EnPassant)
        {
            return game.GetPiece(move.To);
        }

        BoardSquare capturedPawnSquare = new(
            move.From.Rank,
            move.To.File);

        return game.GetPiece(capturedPawnSquare);
    }

    private static int GetSpecialMoveScore(ChessMove move)
    {
        return move.Kind switch
        {
            MoveKind.CastleTowardLowerFile => CastlingPriority,
            MoveKind.CastleTowardHigherFile => CastlingPriority,
            MoveKind.MinisterExchange => ExchangePriority,
            _ => 0
        };
    }

    private static int GetPromotionOrdinal(ChessMove move)
    {
        return move.PromotionPiece.HasValue
            ? (int)move.PromotionPiece.Value + 1
            : 0;
    }
}
