namespace TwelveRanks.Core;

/// <summary>
/// Identifies the chess piece categories currently supported.
/// </summary>
public enum PieceKind
{
    /// <summary>
    /// The king.
    /// </summary>
    King,

    /// <summary>
    /// The queen.
    /// </summary>
    Queen,

    /// <summary>
    /// A rook.
    /// </summary>
    Rook,

    /// <summary>
    /// A bishop.
    /// </summary>
    Bishop,

    /// <summary>
    /// A knight.
    /// </summary>
    Knight,

    /// <summary>
    /// A pawn.
    /// </summary>
    Pawn,

    /// <summary>
    /// A Minister, which combines bishop and knight movement.
    /// </summary>
    Minister,

    /// <summary>
    /// A Chancellor, which combines rook and knight movement.
    /// </summary>
    Chancellor
}
