namespace TwelveRanks.Core.Engine;

internal sealed class SlidingPieceRule((int Rank, int File)[] directions) : IPieceMovementRule
{
    private readonly (int Rank, int File)[] _directions = directions;

    public void AddPseudoLegalMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        List<ChessMove> moves)
    {
        foreach ((int rankDelta, int fileDelta) in _directions)
        {
            AddMovesInDirection(
                position,
                from,
                piece,
                rankDelta,
                fileDelta,
                moves);
        }
    }

    public bool AttacksSquare(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        BoardSquare target)
    {
        _ = piece;

        foreach ((int rankDelta, int fileDelta) in _directions)
        {
            if (AttacksInDirection(
                    position,
                    from,
                    target,
                    rankDelta,
                    fileDelta))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddMovesInDirection(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        int rankDelta,
        int fileDelta,
        List<ChessMove> moves)
    {
        BoardSquare current = from;

        while (current.TryOffset(
                   rankDelta,
                   fileDelta,
                   out BoardSquare target))
        {
            BoardPiece? occupant = position.GetPiece(target);

            if (!occupant.HasValue)
            {
                moves.Add(new ChessMove(from, target));
                current = target;
                continue;
            }

            if (occupant.Value.Side != piece.Side &&
                occupant.Value.Kind != PieceKind.King)
            {
                moves.Add(new ChessMove(from, target, MoveKind.Capture));
            }

            break;
        }
    }

    private static bool AttacksInDirection(
        PositionContext position,
        BoardSquare from,
        BoardSquare target,
        int rankDelta,
        int fileDelta)
    {
        BoardSquare current = from;

        while (current.TryOffset(
                   rankDelta,
                   fileDelta,
                   out BoardSquare candidate))
        {
            if (candidate == target)
            {
                return true;
            }

            if (position.GetPiece(candidate).HasValue)
            {
                return false;
            }

            current = candidate;
        }

        return false;
    }
}
