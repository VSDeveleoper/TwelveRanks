namespace TwelveRanks.Core;

/// <summary>
/// Distinguishes regular pawns from the two Royal Guard pawns.
/// </summary>
public enum PawnRole
{
    /// <summary>
    /// A standard pawn.
    /// </summary>
    Regular,

    /// <summary>
    /// The Royal Guard pawn that begins in front of the queen.
    /// </summary>
    QueenPawn,

    /// <summary>
    /// The Royal Guard pawn that begins in front of the king.
    /// </summary>
    KingPawn
}
