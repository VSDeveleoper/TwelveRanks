namespace TwelveRanks.Core.Engine;

internal sealed class LeapingPieceRule((int Rank, int File)[] offsets) : IPieceMovementRule
{
    private readonly (int Rank, int File)[] _offsets = offsets;

    public void AddPseudoLegalMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        List<ChessMove> moves)
    {
        foreach ((int rankDelta, int fileDelta) in _offsets)
        {
            if (!from.TryOffset(rankDelta, fileDelta, out BoardSquare target))
            {
                continue;
            }

            BoardPiece? occupant = position.GetPiece(target);

            if (!occupant.HasValue)
            {
                moves.Add(new ChessMove(from, target));
            }
            else if (occupant.Value.Side != piece.Side &&
                     occupant.Value.Kind != PieceKind.King)
            {
                moves.Add(new ChessMove(from, target, MoveKind.Capture));
            }
        }
    }

    public bool AttacksSquare(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        BoardSquare target)
    {
        _ = position;
        _ = piece;

        foreach ((int rankDelta, int fileDelta) in _offsets)
        {
            if (from.TryOffset(
                    rankDelta,
                    fileDelta,
                    out BoardSquare candidate) &&
                candidate == target)
            {
                return true;
            }
        }

        return false;
    }
}
