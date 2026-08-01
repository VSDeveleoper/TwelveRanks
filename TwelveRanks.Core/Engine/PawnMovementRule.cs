namespace TwelveRanks.Core.Engine;

internal sealed class PawnMovementRule : IPieceMovementRule
{
    private static readonly PieceKind[] PromotionPieces =
    [
        PieceKind.Queen,
        PieceKind.Rook,
        PieceKind.Bishop,
        PieceKind.Knight,
        PieceKind.Minister,
        PieceKind.Chancellor
    ];

    public void AddPseudoLegalMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        List<ChessMove> moves)
    {
        int forward = piece.Side.ForwardRankDelta();
        bool isRoyalGuard = IsRoyalGuard(piece);

        AddStraightMove(position, from, piece, forward, moves);
        AddForwardDoubleMove(
            position,
            from,
            piece,
            forward,
            isRoyalGuard,
            moves);
        AddDiagonalMoves(position, from, piece, forward, true, moves);

        if (isRoyalGuard)
        {
            AddStraightMove(position, from, piece, -forward, moves);
            AddDiagonalMoves(position, from, piece, -forward, false, moves);
        }
    }

    public bool AttacksSquare(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        BoardSquare target)
    {
        _ = position;

        int rankDelta = target.Rank - from.Rank;
        int fileDelta = Math.Abs(target.File - from.File);
        int forward = piece.Side.ForwardRankDelta();

        if (fileDelta != 1)
        {
            return false;
        }

        return rankDelta == forward ||
            (IsRoyalGuard(piece) && rankDelta == -forward);
    }

    private static void AddStraightMove(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        int rankDelta,
        List<ChessMove> moves)
    {
        if (!from.TryOffset(rankDelta, 0, out BoardSquare target) ||
            position.GetPiece(target).HasValue)
        {
            return;
        }

        AddPawnMove(from, target, piece, false, moves);
    }

    private static void AddForwardDoubleMove(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        int forward,
        bool isRoyalGuard,
        List<ChessMove> moves)
    {
        if (!isRoyalGuard && from.Rank != piece.Side.PawnStartingRank())
        {
            return;
        }

        if (!from.TryOffset(forward, 0, out BoardSquare intermediate) ||
            position.GetPiece(intermediate).HasValue ||
            !intermediate.TryOffset(forward, 0, out BoardSquare target) ||
            position.GetPiece(target).HasValue)
        {
            return;
        }

        AddPawnMove(from, target, piece, false, moves);
    }

    private static void AddDiagonalMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        int rankDelta,
        bool allowEnPassant,
        List<ChessMove> moves)
    {
        AddDiagonalMove(
            position,
            from,
            piece,
            rankDelta,
            -1,
            allowEnPassant,
            moves);
        AddDiagonalMove(
            position,
            from,
            piece,
            rankDelta,
            1,
            allowEnPassant,
            moves);
    }

    private static void AddDiagonalMove(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        int rankDelta,
        int fileDelta,
        bool allowEnPassant,
        List<ChessMove> moves)
    {
        if (!from.TryOffset(
                rankDelta,
                fileDelta,
                out BoardSquare target))
        {
            return;
        }

        BoardPiece? occupant = position.GetPiece(target);

        if (occupant is BoardPiece targetPiece)
        {
            if (targetPiece.Side != piece.Side &&
                targetPiece.Kind != PieceKind.King)
            {
                AddPawnMove(from, target, piece, true, moves);
            }

            return;
        }

        if (allowEnPassant &&
            IsAvailableEnPassant(position, target, piece.Side))
        {
            moves.Add(new ChessMove(from, target, MoveKind.EnPassant));
            return;
        }

        if (IsRoyalGuard(piece))
        {
            AddPawnMove(from, target, piece, false, moves);
        }
    }

    private static bool IsAvailableEnPassant(
        PositionContext position,
        BoardSquare target,
        PieceSide movingSide)
    {
        if (position.EnPassantTarget != target ||
            position.EnPassantPawnSquare is not BoardSquare pawnSquare)
        {
            return false;
        }

        BoardPiece? pawn = position.GetPiece(pawnSquare);

        return pawn is BoardPiece boardPiece &&
            boardPiece.Kind == PieceKind.Pawn &&
            boardPiece.Side == movingSide.Opponent() &&
            pawnSquare.File == target.File;
    }

    private static void AddPawnMove(
        BoardSquare from,
        BoardSquare to,
        BoardPiece pawn,
        bool isCapture,
        List<ChessMove> moves)
    {
        if (IsRoyalGuard(pawn) ||
            to.Rank != pawn.Side.PromotionRank())
        {
            moves.Add(new ChessMove(
                from,
                to,
                isCapture ? MoveKind.Capture : MoveKind.Quiet));
            return;
        }

        MoveKind kind = isCapture
            ? MoveKind.PromotionCapture
            : MoveKind.Promotion;

        foreach (PieceKind promotionPiece in PromotionPieces)
        {
            moves.Add(new ChessMove(from, to, kind, promotionPiece));
        }
    }

    private static bool IsRoyalGuard(BoardPiece piece)
    {
        return piece.PawnRole is
            PawnRole.QueenPawn or
            PawnRole.KingPawn;
    }
}
