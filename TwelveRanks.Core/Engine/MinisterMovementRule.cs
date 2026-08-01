namespace TwelveRanks.Core.Engine;

internal sealed class MinisterMovementRule : IPieceMovementRule
{
    private static readonly (int Rank, int File)[] BishopDirections =
    [
        (-1, -1),
        (-1, 1),
        (1, -1),
        (1, 1)
    ];

    private static readonly (int Rank, int File)[] KnightOffsets =
    [
        (-2, -1),
        (-2, 1),
        (-1, -2),
        (-1, 2),
        (1, -2),
        (1, 2),
        (2, -1),
        (2, 1)
    ];

    private readonly SlidingPieceRule _bishopRule =
        new(BishopDirections);

    private readonly LeapingPieceRule _knightRule =
        new(KnightOffsets);

    public void AddPseudoLegalMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        List<ChessMove> moves)
    {
        _bishopRule.AddPseudoLegalMoves(position, from, piece, moves);
        _knightRule.AddPseudoLegalMoves(position, from, piece, moves);
        AddBishopExchanges(position, from, piece, moves);
        AddKnightExchanges(position, from, piece, moves);
    }

    public bool AttacksSquare(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        BoardSquare target)
    {
        return _bishopRule.AttacksSquare(position, from, piece, target) ||
            _knightRule.AttacksSquare(position, from, piece, target);
    }

    private static void AddBishopExchanges(
        PositionContext position,
        BoardSquare from,
        BoardPiece minister,
        List<ChessMove> moves)
    {
        foreach ((int rankDelta, int fileDelta) in BishopDirections)
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
                    current = target;
                    continue;
                }

                AddExchangeIfEligible(
                    from,
                    target,
                    minister,
                    occupant.Value,
                    moves);
                break;
            }
        }
    }

    private static void AddKnightExchanges(
        PositionContext position,
        BoardSquare from,
        BoardPiece minister,
        List<ChessMove> moves)
    {
        foreach ((int rankDelta, int fileDelta) in KnightOffsets)
        {
            if (!from.TryOffset(
                    rankDelta,
                    fileDelta,
                    out BoardSquare target) ||
                position.GetPiece(target) is not BoardPiece occupant)
            {
                continue;
            }

            AddExchangeIfEligible(
                from,
                target,
                minister,
                occupant,
                moves);
        }
    }

    private static void AddExchangeIfEligible(
        BoardSquare from,
        BoardSquare target,
        BoardPiece minister,
        BoardPiece occupant,
        List<ChessMove> moves)
    {
        if (occupant.Side == minister.Side &&
            occupant.Kind is PieceKind.King or PieceKind.Queen)
        {
            moves.Add(new ChessMove(
                from,
                target,
                MoveKind.MinisterExchange));
        }
    }
}
