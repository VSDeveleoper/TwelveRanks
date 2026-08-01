namespace TwelveRanks.Core;

/// <summary>
/// Identifies a chess piece and the side that owns it.
/// </summary>
public readonly record struct BoardPiece
{
    /// <summary>
    /// Initializes a new piece identity.
    /// </summary>
    /// <param name="kind">The piece's movement category.</param>
    /// <param name="side">The side that owns the piece.</param>
    /// <param name="pawnRole">The pawn's Royal Guard origin role, when applicable.</param>
    public BoardPiece(
        PieceKind kind,
        PieceSide side,
        PawnRole pawnRole = PawnRole.Regular)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The piece kind is not defined.");
        }

        if (!Enum.IsDefined(side))
        {
            throw new ArgumentOutOfRangeException(
                nameof(side),
                side,
                "The piece side is not defined.");
        }

        if (!Enum.IsDefined(pawnRole))
        {
            throw new ArgumentOutOfRangeException(
                nameof(pawnRole),
                pawnRole,
                "The pawn role is not defined.");
        }

        if (kind != PieceKind.Pawn && pawnRole != PawnRole.Regular)
        {
            throw new ArgumentException(
                "Only pawns may have a Royal Guard role.",
                nameof(pawnRole));
        }

        Kind = kind;
        Side = side;
        PawnRole = pawnRole;
    }

    /// <summary>
    /// Gets the piece's movement category.
    /// </summary>
    public PieceKind Kind { get; }

    /// <summary>
    /// Gets the side that owns the piece.
    /// </summary>
    public PieceSide Side { get; }

    /// <summary>
    /// Gets the pawn's Royal Guard origin role.
    /// </summary>
    public PawnRole PawnRole { get; }
}
