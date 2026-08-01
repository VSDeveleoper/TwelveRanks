namespace TwelveRanks.Core.Engine;

internal readonly struct PositionContext(
    BoardPiece?[] board,
    CastlingRights castlingRights,
    BoardSquare? enPassantTarget,
    BoardSquare? enPassantPawnSquare)
{
    public BoardPiece?[] Board { get; } = board;

    public CastlingRights CastlingRights { get; } = castlingRights;

    public BoardSquare? EnPassantTarget { get; } = enPassantTarget;

    public BoardSquare? EnPassantPawnSquare { get; } = enPassantPawnSquare;

    public BoardPiece? GetPiece(BoardSquare square)
    {
        return Board[square.Index];
    }
}
