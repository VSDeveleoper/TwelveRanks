namespace TwelveRanks.Core;

/// <summary>
/// Describes the current rule state of a game.
/// </summary>
public enum GameStatus
{
    /// <summary>
    /// Play may continue and the active king is safe.
    /// </summary>
    Active,

    /// <summary>
    /// Play may continue, but the active king is in check.
    /// </summary>
    Check,

    /// <summary>
    /// The active side is in check and has no legal move.
    /// </summary>
    Checkmate,

    /// <summary>
    /// The active side is not in check and has no legal move.
    /// </summary>
    Stalemate,

    /// <summary>
    /// The same playable position has occurred three times.
    /// </summary>
    DrawByThreefoldRepetition,

    /// <summary>
    /// One hundred half-moves have passed without a pawn move or capture.
    /// </summary>
    DrawByFiftyMoveRule,

    /// <summary>
    /// Neither side has sufficient material to produce checkmate.
    /// </summary>
    DrawByInsufficientMaterial
}
