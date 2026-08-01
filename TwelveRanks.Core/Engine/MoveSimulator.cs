namespace TwelveRanks.Core.Engine;

internal readonly record struct SimulatedMove(
    BoardPiece?[] Board,
    BoardPiece? CapturedPiece,
    BoardSquare? CapturedSquare,
    BoardPiece? ExchangedPiece);

internal static class MoveSimulator
{
    public static SimulatedMove Apply(
        PositionContext position,
        ChessMove move)
    {
        BoardPiece? movingPiece = position.GetPiece(move.From);

        if (!movingPiece.HasValue)
        {
            throw new InvalidOperationException(
                "The source square does not contain a piece.");
        }

        BoardPiece?[] board = (BoardPiece?[])position.Board.Clone();

        if (move.Kind == MoveKind.MinisterExchange)
        {
            return ApplyMinisterExchange(
                board,
                move,
                movingPiece.Value);
        }

        BoardSquare? capturedSquare = GetCapturedSquare(move);
        BoardPiece? capturedPiece = capturedSquare is BoardSquare square
            ? board[square.Index]
            : null;

        board[move.From.Index] = null;

        if (capturedSquare is BoardSquare occupiedSquare)
        {
            board[occupiedSquare.Index] = null;
        }

        if (move.Kind is
            MoveKind.CastleTowardLowerFile or
            MoveKind.CastleTowardHigherFile)
        {
            MoveRookForCastling(board, movingPiece.Value.Side, move.Kind);
        }

        board[move.To.Index] = CreateDestinationPiece(
            movingPiece.Value,
            move);

        return new SimulatedMove(
            board,
            capturedPiece,
            capturedSquare,
            null);
    }

    private static SimulatedMove ApplyMinisterExchange(
        BoardPiece?[] board,
        ChessMove move,
        BoardPiece minister)
    {
        BoardPiece? target = board[move.To.Index];

        if (minister.Kind != PieceKind.Minister ||
            target is not BoardPiece royal ||
            royal.Side != minister.Side ||
            royal.Kind is not PieceKind.King and not PieceKind.Queen)
        {
            throw new InvalidOperationException(
                "A Minister may exchange only with a friendly king or queen.");
        }

        board[move.From.Index] = royal;
        board[move.To.Index] = minister;

        return new SimulatedMove(board, null, null, royal);
    }

    private static BoardSquare? GetCapturedSquare(ChessMove move)
    {
        if (move.Kind == MoveKind.EnPassant)
        {
            return new BoardSquare(move.From.Rank, move.To.File);
        }

        return move.IsCapture ? move.To : null;
    }

    private static BoardPiece CreateDestinationPiece(
        BoardPiece movingPiece,
        ChessMove move)
    {
        return move.PromotionPiece is PieceKind promotionPiece
            ? new BoardPiece(promotionPiece, movingPiece.Side)
            : movingPiece;
    }

    private static void MoveRookForCastling(
        BoardPiece?[] board,
        PieceSide side,
        MoveKind moveKind)
    {
        CastlingGeometry geometry = CastlingGeometry.Get(side, moveKind);
        BoardPiece? rook = board[geometry.RookFrom.Index];

        if (rook is not BoardPiece rookPiece ||
            rookPiece.Kind != PieceKind.Rook ||
            rookPiece.Side != side)
        {
            throw new InvalidOperationException(
                "The castling rook is not available.");
        }

        board[geometry.RookFrom.Index] = null;
        board[geometry.RookTo.Index] = rookPiece;
    }
}
