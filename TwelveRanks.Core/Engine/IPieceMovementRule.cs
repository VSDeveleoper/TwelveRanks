namespace TwelveRanks.Core.Engine;

internal interface IPieceMovementRule
{
    void AddPseudoLegalMoves(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        List<ChessMove> moves);

    bool AttacksSquare(
        PositionContext position,
        BoardSquare from,
        BoardPiece piece,
        BoardSquare target);
}
