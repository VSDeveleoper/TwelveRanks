namespace TwelveRanks.Core;

/// <summary>
/// Describes one complete, engine-generated chess move.
/// </summary>
public readonly record struct ChessMove
{
    /// <summary>
    /// Initializes a chess move.
    /// </summary>
    /// <param name="from">The source square.</param>
    /// <param name="to">The destination square.</param>
    /// <param name="kind">The move operation.</param>
    /// <param name="promotionPiece">The selected promotion piece, when required.</param>
    public ChessMove(
        BoardSquare from,
        BoardSquare to,
        MoveKind kind = MoveKind.Quiet,
        PieceKind? promotionPiece = null)
    {
        if (from == to)
        {
            throw new ArgumentException(
                "A chess move must change squares.",
                nameof(to));
        }

        bool isPromotion = kind is
            MoveKind.Promotion or MoveKind.PromotionCapture;

        if (isPromotion != promotionPiece.HasValue)
        {
            throw new ArgumentException(
                "Promotion moves require one promotion piece, and other moves do not.",
                nameof(promotionPiece));
        }

        if (promotionPiece is PieceKind pieceKind &&
            pieceKind is not PieceKind.Queen and
                not PieceKind.Rook and
                not PieceKind.Bishop and
                not PieceKind.Knight and
                not PieceKind.Minister and
                not PieceKind.Chancellor)
        {
            throw new ArgumentOutOfRangeException(
                nameof(promotionPiece),
                promotionPiece,
                "A regular pawn may promote to a queen, rook, bishop, knight, Minister, or Chancellor.");
        }

        From = from;
        To = to;
        Kind = kind;
        PromotionPiece = promotionPiece;
    }

    /// <summary>
    /// Gets the source square.
    /// </summary>
    public BoardSquare From { get; }

    /// <summary>
    /// Gets the destination square.
    /// </summary>
    public BoardSquare To { get; }

    /// <summary>
    /// Gets the board operation.
    /// </summary>
    public MoveKind Kind { get; }

    /// <summary>
    /// Gets the selected promotion piece.
    /// </summary>
    public PieceKind? PromotionPiece { get; }

    /// <summary>
    /// Gets a value indicating whether this move captures an opposing piece.
    /// </summary>
    public bool IsCapture => Kind is
        MoveKind.Capture or
        MoveKind.EnPassant or
        MoveKind.PromotionCapture;

    /// <summary>
    /// Gets a value indicating whether this move promotes a pawn.
    /// </summary>
    public bool IsPromotion => Kind is
        MoveKind.Promotion or MoveKind.PromotionCapture;

    /// <summary>
    /// Gets a value indicating whether this move exchanges a Minister with a friendly royal piece.
    /// </summary>
    public bool IsExchange => Kind == MoveKind.MinisterExchange;
}
