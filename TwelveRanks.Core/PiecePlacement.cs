namespace TwelveRanks.Core;

/// <summary>
/// Associates a piece with one board square.
/// </summary>
public readonly record struct PiecePlacement
{
    /// <summary>
    /// Initializes a piece placement.
    /// </summary>
    /// <param name="square">The occupied square.</param>
    /// <param name="piece">The occupying piece.</param>
    public PiecePlacement(BoardSquare square, BoardPiece piece)
    {
        Square = square;
        Piece = piece;
    }

    /// <summary>
    /// Gets the occupied square.
    /// </summary>
    public BoardSquare Square { get; }

    /// <summary>
    /// Gets the occupying piece.
    /// </summary>
    public BoardPiece Piece { get; }
}
