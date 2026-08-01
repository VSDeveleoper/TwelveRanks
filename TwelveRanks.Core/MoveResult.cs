namespace TwelveRanks.Core;

/// <summary>
/// Reports the authoritative result of an applied move.
/// </summary>
public sealed class MoveResult
{
    internal MoveResult(
        ChessMove move,
        BoardPiece movingPiece,
        BoardPiece? capturedPiece,
        BoardSquare? capturedSquare,
        BoardPiece? exchangedPiece,
        GameStatus status,
        PieceSide sideToMove)
    {
        Move = move;
        MovingPiece = movingPiece;
        CapturedPiece = capturedPiece;
        CapturedSquare = capturedSquare;
        ExchangedPiece = exchangedPiece;
        Status = status;
        SideToMove = sideToMove;
    }

    /// <summary>
    /// Gets the move that was applied.
    /// </summary>
    public ChessMove Move { get; }

    /// <summary>
    /// Gets the piece before any promotion was applied.
    /// </summary>
    public BoardPiece MovingPiece { get; }

    /// <summary>
    /// Gets the captured piece, if any.
    /// </summary>
    public BoardPiece? CapturedPiece { get; }

    /// <summary>
    /// Gets the square from which the captured piece was removed.
    /// </summary>
    public BoardSquare? CapturedSquare { get; }

    /// <summary>
    /// Gets the friendly king or queen exchanged with the moving Minister, if any.
    /// </summary>
    public BoardPiece? ExchangedPiece { get; }

    /// <summary>
    /// Gets the game status after the move.
    /// </summary>
    public GameStatus Status { get; }

    /// <summary>
    /// Gets the side whose turn follows the move.
    /// </summary>
    public PieceSide SideToMove { get; }
}
