namespace TwelveRanks.Core.Engine;

internal sealed class ChancellorMovementRule : IPieceMovementRule
{
    private readonly IPieceMovementRule _rookRule;
    private readonly IPieceMovementRule _knightRule;

    public ChancellorMovementRule(
        IPieceMovementRule rookRule,
        IPieceMovementRule knightRule)
    {
        ArgumentNullException.ThrowIfNull(rookRule);
        ArgumentNullException.ThrowIfNull(knightRule);

        _rookRule = rookRule;
        _knightRule = knightRule;
    }

    public void AddPseudoLegalMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        List<ChessMove> moves)
    {
        _rookRule.AddPseudoLegalMoves(position, from, piece, moves);
        _knightRule.AddPseudoLegalMoves(position, from, piece, moves);
    }

    public bool AttacksSquare(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        BoardSquare target)
    {
        return _rookRule.AttacksSquare(position, from, piece, target) ||
            _knightRule.AttacksSquare(position, from, piece, target);
    }
}
