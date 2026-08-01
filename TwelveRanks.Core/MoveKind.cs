namespace TwelveRanks.Core;

/// <summary>
/// Describes the board operation performed by a chess move.
/// </summary>
public enum MoveKind
{
    /// <summary>
    /// A non-capturing move.
    /// </summary>
    Quiet,

    /// <summary>
    /// A move that captures the piece on its destination.
    /// </summary>
    Capture,

    /// <summary>
    /// A forward-diagonal pawn capture of a just-advanced pawn.
    /// </summary>
    EnPassant,

    /// <summary>
    /// Castling toward file zero.
    /// </summary>
    CastleTowardLowerFile,

    /// <summary>
    /// Castling toward file eleven.
    /// </summary>
    CastleTowardHigherFile,

    /// <summary>
    /// A non-capturing pawn promotion.
    /// </summary>
    Promotion,

    /// <summary>
    /// A capturing pawn promotion.
    /// </summary>
    PromotionCapture,

    /// <summary>
    /// An atomic position exchange between a Minister and a friendly king or queen.
    /// </summary>
    MinisterExchange
}
