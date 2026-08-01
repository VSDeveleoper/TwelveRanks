namespace TwelveRanks.Core;

/// <summary>
/// Identifies the castling moves that remain available.
/// </summary>
[Flags]
public enum CastlingRights
{
    /// <summary>
    /// No castling move remains available.
    /// </summary>
    None = 0,

    /// <summary>
    /// White may castle toward file zero.
    /// </summary>
    WhiteTowardLowerFile = 1,

    /// <summary>
    /// White may castle toward file eleven.
    /// </summary>
    WhiteTowardHigherFile = 2,

    /// <summary>
    /// Black may castle toward file zero.
    /// </summary>
    BlackTowardLowerFile = 4,

    /// <summary>
    /// Black may castle toward file eleven.
    /// </summary>
    BlackTowardHigherFile = 8,

    /// <summary>
    /// Both sides retain both castling moves.
    /// </summary>
    All = WhiteTowardLowerFile |
        WhiteTowardHigherFile |
        BlackTowardLowerFile |
        BlackTowardHigherFile
}
