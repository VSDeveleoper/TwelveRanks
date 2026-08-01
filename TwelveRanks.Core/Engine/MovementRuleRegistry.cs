namespace TwelveRanks.Core.Engine;

internal static class MovementRuleRegistry
{
    private static readonly IPieceMovementRule RookRule =
        new SlidingPieceRule(
        [
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1)
        ]);

    private static readonly IPieceMovementRule BishopRule =
        new SlidingPieceRule(
        [
            (-1, -1),
            (-1, 1),
            (1, -1),
            (1, 1)
        ]);

    private static readonly IPieceMovementRule QueenRule =
        new SlidingPieceRule(
        [
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1),
            (-1, -1),
            (-1, 1),
            (1, -1),
            (1, 1)
        ]);

    private static readonly IPieceMovementRule KnightRule =
        new LeapingPieceRule(
        [
            (-2, -1),
            (-2, 1),
            (-1, -2),
            (-1, 2),
            (1, -2),
            (1, 2),
            (2, -1),
            (2, 1)
        ]);

    private static readonly IPieceMovementRule KingRule =
        new LeapingPieceRule(
        [
            (-1, -1),
            (-1, 0),
            (-1, 1),
            (0, -1),
            (0, 1),
            (1, -1),
            (1, 0),
            (1, 1)
        ]);

    private static readonly IPieceMovementRule PawnRule =
        new PawnMovementRule();

    private static readonly IPieceMovementRule MinisterRule =
        new MinisterMovementRule();

    private static readonly IPieceMovementRule ChancellorRule =
        new ChancellorMovementRule(RookRule, KnightRule);

    public static IPieceMovementRule Get(PieceKind pieceKind)
    {
        return pieceKind switch
        {
            PieceKind.King => KingRule,
            PieceKind.Queen => QueenRule,
            PieceKind.Rook => RookRule,
            PieceKind.Bishop => BishopRule,
            PieceKind.Knight => KnightRule,
            PieceKind.Pawn => PawnRule,
            PieceKind.Minister => MinisterRule,
            PieceKind.Chancellor => ChancellorRule,
            _ => throw new ArgumentOutOfRangeException(
                nameof(pieceKind),
                pieceKind,
                "No movement rule is registered for this piece.")
        };
    }
}
