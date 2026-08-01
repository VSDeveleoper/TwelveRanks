using TwelveRanks.Core.Engine;
using System.Text;

namespace TwelveRanks.Core;

/// <summary>
/// Owns one authoritative Twelve Ranks position and applies only legal moves.
/// </summary>
public sealed class ChessGame
{
    private readonly Dictionary<string, int> _positionOccurrences =
        new(StringComparer.Ordinal);

    private readonly List<MoveResult> _moveHistory = [];
    private readonly Stack<RedoEntry> _redoHistory = [];
    private readonly Stack<GameSnapshot> _undoHistory = [];

    private BoardPiece?[] _board;
    private CastlingRights _castlingRights;
    private BoardSquare? _enPassantTarget;
    private BoardSquare? _enPassantPawnSquare;
    private PieceSide? _resignedSide;
    private int _halfMoveClock;
    private int _fullMoveNumber;

    private ChessGame(
        IEnumerable<PiecePlacement> placements,
        PieceSide sideToMove,
        CastlingRights castlingRights)
    {
        ArgumentNullException.ThrowIfNull(placements);

        _board = new BoardPiece?[BoardSpecification.SquareCount];
        SideToMove = sideToMove;
        _castlingRights = castlingRights;
        _fullMoveNumber = 1;

        PlacePieces(placements);
        ValidateKings();
        RecordCurrentPosition();
        Status = EvaluateStatus();
    }

    private ChessGame(ChessGame source)
    {
        _board = (BoardPiece?[])source._board.Clone();
        _castlingRights = source._castlingRights;
        _enPassantTarget = source._enPassantTarget;
        _enPassantPawnSquare = source._enPassantPawnSquare;
        _resignedSide = source._resignedSide;
        _halfMoveClock = source._halfMoveClock;
        _fullMoveNumber = source._fullMoveNumber;
        SideToMove = source.SideToMove;
        Status = source.Status;

        foreach (KeyValuePair<string, int> occurrence in
                 source._positionOccurrences)
        {
            _positionOccurrences.Add(occurrence.Key, occurrence.Value);
        }

        _moveHistory.AddRange(source._moveHistory);
    }

    /// <summary>
    /// Gets the side that may make the next move.
    /// </summary>
    public PieceSide SideToMove { get; private set; }

    /// <summary>
    /// Gets the current rule status.
    /// </summary>
    public GameStatus Status { get; private set; }

    /// <summary>
    /// Gets the castling moves that remain available.
    /// </summary>
    public CastlingRights CastlingRights => _castlingRights;

    /// <summary>
    /// Gets the current en-passant destination, when one exists.
    /// </summary>
    public BoardSquare? EnPassantTarget => _enPassantTarget;

    /// <summary>
    /// Gets the number of half-moves since the last pawn move or capture.
    /// </summary>
    public int HalfMoveClock => _halfMoveClock;

    /// <summary>
    /// Gets the one-based full-move number.
    /// </summary>
    public int FullMoveNumber => _fullMoveNumber;

    /// <summary>
    /// Gets a value indicating whether at least one move can be undone.
    /// </summary>
    public bool CanUndo =>
        !_resignedSide.HasValue && _undoHistory.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one undone move can be redone.
    /// </summary>
    public bool CanRedo =>
        !_resignedSide.HasValue && _redoHistory.Count > 0;

    /// <summary>
    /// Gets the side that resigned, when the game ended by resignation.
    /// </summary>
    public PieceSide? ResignedSide => _resignedSide;

    /// <summary>
    /// Gets a value indicating whether the game has reached a terminal result.
    /// </summary>
    public bool IsGameOver =>
        _resignedSide.HasValue || IsTerminal(Status);

    /// <summary>
    /// Gets the winner after checkmate or resignation.
    /// </summary>
    public PieceSide? Winner => _resignedSide is PieceSide resignedSide
        ? resignedSide.Opponent()
        : Status == GameStatus.Checkmate
            ? SideToMove.Opponent()
            : null;

    /// <summary>
    /// Gets the successfully applied moves in chronological order.
    /// </summary>
    public IReadOnlyList<MoveResult> MoveHistory =>
        _moveHistory.AsReadOnly();

    /// <summary>
    /// Creates a game with the standard Twelve Ranks starting formation.
    /// </summary>
    /// <returns>A new game with White to move.</returns>
    public static ChessGame CreateInitial()
    {
        return new ChessGame(
            InitialPosition.Create(),
            PieceSide.White,
            CastlingRights.All);
    }

    /// <summary>
    /// Creates a validated custom setup position, primarily for tests and analysis.
    /// </summary>
    /// <param name="placements">The pieces and their occupied squares.</param>
    /// <param name="sideToMove">The side that may make the next move.</param>
    /// <param name="castlingRights">The castling rights to preserve.</param>
    /// <returns>A game containing the supplied position.</returns>
    public static ChessGame CreateCustom(
        IEnumerable<PiecePlacement> placements,
        PieceSide sideToMove = PieceSide.White,
        CastlingRights castlingRights = CastlingRights.None)
    {
        return new ChessGame(placements, sideToMove, castlingRights);
    }

    /// <summary>
    /// Creates an independent copy suitable for opponent analysis.
    /// </summary>
    /// <returns>A copy whose future moves do not affect this game.</returns>
    public ChessGame Clone()
    {
        return new ChessGame(this);
    }

    /// <summary>
    /// Gets the piece occupying one square.
    /// </summary>
    /// <param name="square">The square to inspect.</param>
    /// <returns>The piece, or no value when the square is empty.</returns>
    public BoardPiece? GetPiece(BoardSquare square)
    {
        return _board[square.Index];
    }

    /// <summary>
    /// Gets a snapshot of all occupied squares.
    /// </summary>
    /// <returns>The current piece placements.</returns>
    public IReadOnlyList<PiecePlacement> GetPieces()
    {
        List<PiecePlacement> placements = [];

        for (int index = 0; index < BoardSpecification.SquareCount; index++)
        {
            if (_board[index] is BoardPiece piece)
            {
                placements.Add(new PiecePlacement(
                    BoardSquare.FromIndex(index),
                    piece));
            }
        }

        return placements;
    }

    /// <summary>
    /// Gets every legal move for the active side.
    /// </summary>
    /// <returns>The legal moves, or an empty list after the game ends.</returns>
    public IReadOnlyList<ChessMove> GetLegalMoves()
    {
        return IsGameOver
            ? []
            : MoveGenerator.GetLegalMoves(CreateContext(), SideToMove);
    }

    /// <summary>
    /// Gets the legal moves for the active side's piece on one square.
    /// </summary>
    /// <param name="from">The piece's source square.</param>
    /// <returns>The legal moves from that square.</returns>
    public IReadOnlyList<ChessMove> GetLegalMoves(BoardSquare from)
    {
        return IsGameOver
            ? []
            : MoveGenerator.GetLegalMoves(
                CreateContext(),
                from,
                SideToMove);
    }

    /// <summary>
    /// Determines whether one side's king is currently attacked.
    /// </summary>
    /// <param name="side">The side whose king should be examined.</param>
    /// <returns><see langword="true"/> when that king is in check.</returns>
    public bool IsInCheck(PieceSide side)
    {
        return MoveGenerator.IsKingInCheck(CreateContext(), side);
    }

    /// <summary>
    /// Attempts to apply one exact engine move.
    /// </summary>
    /// <param name="move">The candidate move.</param>
    /// <param name="result">The applied result when successful.</param>
    /// <returns><see langword="true"/> when the move was legal and applied.</returns>
    public bool TryMakeMove(
        ChessMove move,
        out MoveResult? result)
    {
        result = null;

        if (IsGameOver ||
            !FindMatchingLegalMove(move, out ChessMove legalMove))
        {
            return false;
        }

        BoardPiece movingPiece = _board[legalMove.From.Index]!.Value;
        GameSnapshot snapshot = CaptureSnapshot();
        SimulatedMove simulatedMove =
            MoveSimulator.Apply(CreateContext(), legalMove);

        UpdateCastlingRights(
            movingPiece,
            legalMove,
            simulatedMove.CapturedPiece,
            simulatedMove.CapturedSquare,
            simulatedMove.ExchangedPiece);
        UpdateClocks(movingPiece, simulatedMove.CapturedPiece);
        UpdateEnPassant(movingPiece, legalMove);

        _board = simulatedMove.Board;

        if (SideToMove == PieceSide.Black)
        {
            _fullMoveNumber++;
        }

        SideToMove = SideToMove.Opponent();
        RecordCurrentPosition();
        Status = EvaluateStatus();

        result = new MoveResult(
            legalMove,
            movingPiece,
            simulatedMove.CapturedPiece,
            simulatedMove.CapturedSquare,
            simulatedMove.ExchangedPiece,
            Status,
            SideToMove);

        _redoHistory.Clear();
        _undoHistory.Push(snapshot);
        _moveHistory.Add(result);
        return true;
    }

    /// <summary>
    /// Ends the game because one side resigned.
    /// </summary>
    /// <param name="side">The side conceding the game.</param>
    /// <returns>
    /// <see langword="true"/> when the resignation ended an active game.
    /// </returns>
    public bool Resign(PieceSide side)
    {
        if (IsGameOver)
        {
            return false;
        }

        _resignedSide = side;
        _redoHistory.Clear();
        return true;
    }

    /// <summary>
    /// Restores the position immediately before the last applied move.
    /// </summary>
    /// <returns><see langword="true"/> when a move was undone.</returns>
    public bool UndoLastMove()
    {
        if (!CanUndo)
        {
            return false;
        }

        GameSnapshot redoSnapshot = CaptureSnapshot();
        MoveResult undoneMove = _moveHistory[^1];

        DecrementCurrentPosition();
        GameSnapshot snapshot = _undoHistory.Pop();

        RestoreSnapshot(snapshot);
        _moveHistory.RemoveAt(_moveHistory.Count - 1);
        _redoHistory.Push(new RedoEntry(redoSnapshot, undoneMove));
        return true;
    }

    /// <summary>
    /// Restores the next move in the current redo history.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an undone move was restored.
    /// </returns>
    public bool RedoLastMove()
    {
        if (!CanRedo)
        {
            return false;
        }

        GameSnapshot undoSnapshot = CaptureSnapshot();
        RedoEntry entry = _redoHistory.Pop();

        RestoreSnapshot(entry.Snapshot);
        RecordCurrentPosition();
        _undoHistory.Push(undoSnapshot);
        _moveHistory.Add(entry.Move);
        return true;
    }

    /// <summary>
    /// Gets the pieces captured by one side.
    /// </summary>
    /// <param name="capturingSide">The side that made the captures.</param>
    /// <returns>The captured pieces in chronological order.</returns>
    public IReadOnlyList<BoardPiece> GetCapturedPieces(
        PieceSide capturingSide)
    {
        List<BoardPiece> pieces = [];

        foreach (MoveResult move in _moveHistory)
        {
            if (move.MovingPiece.Side == capturingSide &&
                move.CapturedPiece is BoardPiece capturedPiece)
            {
                pieces.Add(capturedPiece);
            }
        }

        return pieces;
    }

    private PositionContext CreateContext()
    {
        return new PositionContext(
            _board,
            _castlingRights,
            _enPassantTarget,
            _enPassantPawnSquare);
    }

    private void PlacePieces(IEnumerable<PiecePlacement> placements)
    {
        foreach (PiecePlacement placement in placements)
        {
            if (_board[placement.Square.Index].HasValue)
            {
                throw new ArgumentException(
                    $"Square {placement.Square} contains more than one piece.",
                    nameof(placements));
            }

            _board[placement.Square.Index] = placement.Piece;
        }
    }

    private void ValidateKings()
    {
        ValidateKing(PieceSide.White);
        ValidateKing(PieceSide.Black);
    }

    private void ValidateKing(PieceSide side)
    {
        int kingCount = 0;

        foreach (BoardPiece? piece in _board)
        {
            if (piece is BoardPiece boardPiece &&
                boardPiece.Kind == PieceKind.King &&
                boardPiece.Side == side)
            {
                kingCount++;
            }
        }

        if (kingCount != 1)
        {
            throw new ArgumentException(
                $"A position must contain exactly one {side} king.");
        }
    }

    private bool FindMatchingLegalMove(
        ChessMove candidate,
        out ChessMove legalMove)
    {
        foreach (ChessMove move in GetLegalMoves(candidate.From))
        {
            if (move == candidate)
            {
                legalMove = move;
                return true;
            }
        }

        legalMove = default;
        return false;
    }

    private GameSnapshot CaptureSnapshot()
    {
        return new GameSnapshot(
            (BoardPiece?[])_board.Clone(),
            SideToMove,
            _castlingRights,
            _enPassantTarget,
            _enPassantPawnSquare,
            _resignedSide,
            _halfMoveClock,
            _fullMoveNumber,
            Status);
    }

    private void RestoreSnapshot(GameSnapshot snapshot)
    {
        _board = snapshot.Board;
        _castlingRights = snapshot.CastlingRights;
        _enPassantTarget = snapshot.EnPassantTarget;
        _enPassantPawnSquare = snapshot.EnPassantPawnSquare;
        _resignedSide = snapshot.ResignedSide;
        _halfMoveClock = snapshot.HalfMoveClock;
        _fullMoveNumber = snapshot.FullMoveNumber;
        SideToMove = snapshot.SideToMove;
        Status = snapshot.Status;
    }

    private void UpdateClocks(
        BoardPiece movingPiece,
        BoardPiece? capturedPiece)
    {
        _halfMoveClock =
            movingPiece.Kind == PieceKind.Pawn || capturedPiece.HasValue
                ? 0
                : _halfMoveClock + 1;
    }

    private void UpdateEnPassant(
        BoardPiece movingPiece,
        ChessMove move)
    {
        _enPassantTarget = null;
        _enPassantPawnSquare = null;

        if (movingPiece.Kind != PieceKind.Pawn ||
            move.IsPromotion ||
            Math.Abs(move.To.Rank - move.From.Rank) != 2)
        {
            return;
        }

        _enPassantTarget = new BoardSquare(
            (move.From.Rank + move.To.Rank) / 2,
            move.From.File);
        _enPassantPawnSquare = move.To;
    }

    private void UpdateCastlingRights(
        BoardPiece movingPiece,
        ChessMove move,
        BoardPiece? capturedPiece,
        BoardSquare? capturedSquare,
        BoardPiece? exchangedPiece)
    {
        if (movingPiece.Kind == PieceKind.King)
        {
            ClearCastlingRights(movingPiece.Side);
        }
        else if (movingPiece.Kind == PieceKind.Rook)
        {
            ClearRookCastlingRight(movingPiece.Side, move.From);
        }

        if (exchangedPiece is BoardPiece exchangedKing &&
            exchangedKing.Kind == PieceKind.King)
        {
            ClearCastlingRights(exchangedKing.Side);
        }

        if (capturedPiece is BoardPiece capturedRook &&
            capturedRook.Kind == PieceKind.Rook &&
            capturedSquare is BoardSquare rookSquare)
        {
            ClearRookCastlingRight(capturedRook.Side, rookSquare);
        }
    }

    private void ClearCastlingRights(PieceSide side)
    {
        CastlingRights rights = side == PieceSide.White
            ? CastlingRights.WhiteTowardLowerFile |
                CastlingRights.WhiteTowardHigherFile
            : CastlingRights.BlackTowardLowerFile |
                CastlingRights.BlackTowardHigherFile;

        _castlingRights &= ~rights;
    }

    private void ClearRookCastlingRight(
        PieceSide side,
        BoardSquare square)
    {
        if (square.Rank != side.HomeRank())
        {
            return;
        }

        if (square.File == 0)
        {
            _castlingRights &= side == PieceSide.White
                ? ~CastlingRights.WhiteTowardLowerFile
                : ~CastlingRights.BlackTowardLowerFile;
        }
        else if (square.File == BoardSpecification.Files - 1)
        {
            _castlingRights &= side == PieceSide.White
                ? ~CastlingRights.WhiteTowardHigherFile
                : ~CastlingRights.BlackTowardHigherFile;
        }
    }

    private GameStatus EvaluateStatus()
    {
        PositionContext position = CreateContext();
        bool isInCheck =
            MoveGenerator.IsKingInCheck(position, SideToMove);
        bool hasLegalMove =
            MoveGenerator.GetLegalMoves(position, SideToMove).Count > 0;

        if (!hasLegalMove)
        {
            return isInCheck
                ? GameStatus.Checkmate
                : GameStatus.Stalemate;
        }

        if (_halfMoveClock >= 100)
        {
            return GameStatus.DrawByFiftyMoveRule;
        }

        string positionKey = CreatePositionKey();

        if (_positionOccurrences.TryGetValue(
                positionKey,
                out int occurrences) &&
            occurrences >= 3)
        {
            return GameStatus.DrawByThreefoldRepetition;
        }

        if (HasInsufficientMaterial())
        {
            return GameStatus.DrawByInsufficientMaterial;
        }

        return isInCheck ? GameStatus.Check : GameStatus.Active;
    }

    private bool HasInsufficientMaterial()
    {
        List<PiecePlacement> nonKings = [];

        foreach (PiecePlacement placement in GetPieces())
        {
            if (placement.Piece.Kind != PieceKind.King)
            {
                nonKings.Add(placement);
            }
        }

        if (nonKings.Count == 0)
        {
            return true;
        }

        if (nonKings.Count == 1)
        {
            PieceKind kind = nonKings[0].Piece.Kind;
            return kind is PieceKind.Bishop or PieceKind.Knight;
        }

        return ContainsOnlySameColorBishops(nonKings);
    }

    private static bool ContainsOnlySameColorBishops(
        IReadOnlyList<PiecePlacement> pieces)
    {
        int? squareColor = null;

        foreach (PiecePlacement placement in pieces)
        {
            if (placement.Piece.Kind != PieceKind.Bishop)
            {
                return false;
            }

            int color =
                (placement.Square.Rank + placement.Square.File) % 2;

            if (squareColor.HasValue && squareColor.Value != color)
            {
                return false;
            }

            squareColor = color;
        }

        return true;
    }

    private void RecordCurrentPosition()
    {
        string positionKey = CreatePositionKey();

        _positionOccurrences[positionKey] =
            _positionOccurrences.TryGetValue(positionKey, out int count)
                ? count + 1
                : 1;
    }

    private void DecrementCurrentPosition()
    {
        string positionKey = CreatePositionKey();

        if (!_positionOccurrences.TryGetValue(
                positionKey,
                out int count))
        {
            return;
        }

        if (count <= 1)
        {
            _ = _positionOccurrences.Remove(positionKey);
        }
        else
        {
            _positionOccurrences[positionKey] = count - 1;
        }
    }

    private BoardSquare? GetRepetitionEnPassantTarget()
    {
        if (_enPassantTarget is not BoardSquare target)
        {
            return null;
        }

        return HasLegalEnPassantCapture(target, -1) ||
            HasLegalEnPassantCapture(target, 1)
                ? target
                : null;
    }

    private bool HasLegalEnPassantCapture(
        BoardSquare target,
        int sourceFileDelta)
    {
        if (!target.TryOffset(
                -SideToMove.ForwardRankDelta(),
                sourceFileDelta,
                out BoardSquare source))
        {
            return false;
        }

        foreach (ChessMove move in MoveGenerator.GetLegalMoves(
                     CreateContext(),
                     source,
                     SideToMove))
        {
            if (move.Kind == MoveKind.EnPassant && move.To == target)
            {
                return true;
            }
        }

        return false;
    }

    private string CreatePositionKey()
    {
        StringBuilder builder = new(BoardSpecification.SquareCount + 8);

        foreach (BoardPiece? piece in _board)
        {
            if (piece is not BoardPiece boardPiece)
            {
                _ = builder.Append('.');
                continue;
            }

            _ = builder.Append((char)('A' + (int)boardPiece.Kind));
            _ = builder.Append(
                boardPiece.Side == PieceSide.White ? 'W' : 'B');
            _ = builder.Append(
                (char)('0' + (int)boardPiece.PawnRole));
        }

        _ = builder.Append('|');
        _ = builder.Append(SideToMove == PieceSide.White ? 'W' : 'B');
        _ = builder.Append((char)('A' + (int)_castlingRights));
        _ = builder.Append(GetRepetitionEnPassantTarget() is BoardSquare target
            ? (char)(target.Index + 1)
            : char.MinValue);

        return builder.ToString();
    }

    private static bool IsTerminal(GameStatus status)
    {
        return status is
            GameStatus.Checkmate or
            GameStatus.Stalemate or
            GameStatus.DrawByThreefoldRepetition or
            GameStatus.DrawByFiftyMoveRule or
            GameStatus.DrawByInsufficientMaterial;
    }

    private readonly record struct GameSnapshot(
        BoardPiece?[] Board,
        PieceSide SideToMove,
        CastlingRights CastlingRights,
        BoardSquare? EnPassantTarget,
        BoardSquare? EnPassantPawnSquare,
        PieceSide? ResignedSide,
        int HalfMoveClock,
        int FullMoveNumber,
        GameStatus Status);

    private readonly record struct RedoEntry(
        GameSnapshot Snapshot,
        MoveResult Move);
}
