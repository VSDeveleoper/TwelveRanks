namespace TwelveRanks.Core.Engine;

internal static class MoveGenerator
{
    public static IReadOnlyList<ChessMove> GetLegalMoves(
        PositionContext position,
        PieceSide side)
    {
        List<ChessMove> moves = [];

        for (int index = 0; index < BoardSpecification.SquareCount; index++)
        {
            BoardPiece? piece = position.Board[index];

            if (piece is BoardPiece boardPiece &&
                boardPiece.Side == side)
            {
                AddLegalMovesFrom(
                    position,
                    BoardSquare.FromIndex(index),
                    boardPiece,
                    moves);
            }
        }

        return moves;
    }

    public static IReadOnlyList<ChessMove> GetLegalMoves(
        PositionContext position,
        BoardSquare from,
        PieceSide side)
    {
        BoardPiece? piece = position.GetPiece(from);

        if (piece is not BoardPiece boardPiece ||
            boardPiece.Side != side)
        {
            return [];
        }

        List<ChessMove> moves = [];
        AddLegalMovesFrom(position, from, boardPiece, moves);
        return moves;
    }

    public static bool IsKingInCheck(
        PositionContext position,
        PieceSide side)
    {
        BoardSquare kingSquare = FindKing(position, side);
        return IsSquareAttacked(position, kingSquare, side.Opponent());
    }

    public static bool IsSquareAttacked(
        PositionContext position,
        BoardSquare target,
        PieceSide attackingSide)
    {
        for (int index = 0; index < BoardSpecification.SquareCount; index++)
        {
            BoardPiece? piece = position.Board[index];

            if (piece is not BoardPiece attackingPiece ||
                attackingPiece.Side != attackingSide)
            {
                continue;
            }

            IPieceMovementRule rule =
                MovementRuleRegistry.Get(attackingPiece.Kind);

            if (rule.AttacksSquare(
                    position,
                    BoardSquare.FromIndex(index),
                    attackingPiece,
                    target))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddLegalMovesFrom(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        List<ChessMove> legalMoves)
    {
        List<ChessMove> candidates = [];
        IPieceMovementRule rule = MovementRuleRegistry.Get(piece.Kind);

        rule.AddPseudoLegalMoves(position, from, piece, candidates);

        if (piece.Kind == PieceKind.King)
        {
            AddCastlingMoves(position, from, piece, candidates);
        }

        foreach (ChessMove candidate in candidates)
        {
            if (PreservesKing(position, candidate, piece.Side))
            {
                legalMoves.Add(candidate);
            }
        }
    }

    private static bool PreservesKing(
        PositionContext position,
        ChessMove move,
        PieceSide movingSide)
    {
        SimulatedMove simulatedMove = MoveSimulator.Apply(position, move);
        PositionContext simulatedPosition = new(
            simulatedMove.Board,
            position.CastlingRights,
            null,
            null);

        return !IsKingInCheck(simulatedPosition, movingSide);
    }

    private static BoardSquare FindKing(
        PositionContext position,
        PieceSide side)
    {
        for (int index = 0; index < BoardSpecification.SquareCount; index++)
        {
            BoardPiece? piece = position.Board[index];

            if (piece is BoardPiece boardPiece &&
                boardPiece.Side == side &&
                boardPiece.Kind == PieceKind.King)
            {
                return BoardSquare.FromIndex(index);
            }
        }

        throw new InvalidOperationException(
            $"The position does not contain the {side} king.");
    }

    private static void AddCastlingMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece king,
        List<ChessMove> moves)
    {
        TryAddCastlingMove(
            position,
            from,
            king,
            MoveKind.CastleTowardLowerFile,
            moves);
        TryAddCastlingMove(
            position,
            from,
            king,
            MoveKind.CastleTowardHigherFile,
            moves);
    }

    private static void TryAddCastlingMove(
        PositionContext position,
        BoardSquare from,
        BoardPiece king,
        MoveKind moveKind,
        List<ChessMove> moves)
    {
        CastlingGeometry geometry =
            CastlingGeometry.Get(king.Side, moveKind);

        if (from != geometry.KingFrom ||
            !position.CastlingRights.HasFlag(geometry.RequiredRight) ||
            !HasEligibleRook(position, geometry.RookFrom, king.Side) ||
            !IsPathClear(position, geometry.KingFrom, geometry.RookFrom) ||
            IsSquareAttacked(position, from, king.Side.Opponent()) ||
            !IsTransitSquareSafe(position, geometry, king))
        {
            return;
        }

        moves.Add(new ChessMove(from, geometry.KingTo, moveKind));
    }

    private static bool HasEligibleRook(
        PositionContext position,
        BoardSquare rookSquare,
        PieceSide side)
    {
        BoardPiece? piece = position.GetPiece(rookSquare);

        return piece is BoardPiece rook &&
            rook.Kind == PieceKind.Rook &&
            rook.Side == side;
    }

    private static bool IsPathClear(
        PositionContext position,
        BoardSquare kingSquare,
        BoardSquare rookSquare)
    {
        int direction = rookSquare.File < kingSquare.File ? -1 : 1;

        for (int file = kingSquare.File + direction;
             file != rookSquare.File;
             file += direction)
        {
            if (position.GetPiece(
                    new BoardSquare(kingSquare.Rank, file)).HasValue)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsTransitSquareSafe(
        PositionContext position,
        CastlingGeometry geometry,
        BoardPiece king)
    {
        BoardPiece?[] board = (BoardPiece?[])position.Board.Clone();
        board[geometry.KingFrom.Index] = null;
        board[geometry.KingThrough.Index] = king;

        PositionContext transitPosition = new(
            board,
            position.CastlingRights,
            null,
            null);

        return !IsSquareAttacked(
            transitPosition,
            geometry.KingThrough,
            king.Side.Opponent());
    }
}
