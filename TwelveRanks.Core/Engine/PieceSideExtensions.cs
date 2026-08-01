namespace TwelveRanks.Core.Engine;

internal static class PieceSideExtensions
{
    public static PieceSide Opponent(this PieceSide side)
    {
        return side == PieceSide.White
            ? PieceSide.Black
            : PieceSide.White;
    }

    public static int ForwardRankDelta(this PieceSide side)
    {
        return side == PieceSide.White ? -1 : 1;
    }

    public static int HomeRank(this PieceSide side)
    {
        return side == PieceSide.White
            ? BoardSpecification.Ranks - 1
            : 0;
    }

    public static int PawnStartingRank(this PieceSide side)
    {
        return side == PieceSide.White
            ? BoardSpecification.Ranks - 2
            : 1;
    }

    public static int PromotionRank(this PieceSide side)
    {
        return side == PieceSide.White
            ? 0
            : BoardSpecification.Ranks - 1;
    }
}
