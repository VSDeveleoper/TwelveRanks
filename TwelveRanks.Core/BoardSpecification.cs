namespace TwelveRanks.Core;

/// <summary>
/// Defines the fixed dimensions of a Twelve Ranks board.
/// </summary>
public static class BoardSpecification
{
    /// <summary>
    /// The number of files on the board.
    /// </summary>
    public static readonly int Files = 12;

    /// <summary>
    /// The number of ranks on the board.
    /// </summary>
    public static readonly int Ranks = 12;

    /// <summary>
    /// The total number of board squares.
    /// </summary>
    public static readonly int SquareCount = Files * Ranks;
}
