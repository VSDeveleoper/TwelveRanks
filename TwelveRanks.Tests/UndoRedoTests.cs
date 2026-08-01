using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies reversible navigation through an authoritative game history.
/// </summary>
[TestClass]
public sealed class UndoRedoTests
{
    /// <summary>
    /// Verifies a complete rewind and replay of a multi-move opening line,
    /// including both history boundaries.
    /// </summary>
    [TestMethod]
    public void FullRewindAndRedoRestoreTheCompleteOpeningLine()
    {
        ChessGame game = ChessGame.CreateInitial();
        ObservableGameState initialState = CaptureState(game);

        Assert.IsFalse(game.CanUndo);
        Assert.IsFalse(game.CanRedo);

        ApplyOpeningLine(game);

        ObservableGameState completedState = CaptureState(game);
        Assert.IsTrue(game.CanUndo);
        Assert.IsFalse(game.CanRedo);

        UndoMoves(game, count: 4);

        AssertState(initialState, game);
        Assert.IsFalse(game.CanUndo);
        Assert.IsTrue(game.CanRedo);

        AssertUndoBoundary(game);

        RedoMoves(game, count: 4);

        AssertState(completedState, game);
        Assert.IsTrue(game.CanUndo);
        Assert.IsFalse(game.CanRedo);

        AssertRedoBoundary(game);
    }

    /// <summary>
    /// Verifies that a new legal move after an undo creates a new branch and
    /// discards the abandoned redo line.
    /// </summary>
    [TestMethod]
    public void NewMoveAfterUndoClearsTheRedoBranch()
    {
        ChessGame game = ChessGame.CreateInitial();
        Apply(game, new BoardSquare(10, 0), new BoardSquare(8, 0));
        Apply(game, new BoardSquare(1, 0), new BoardSquare(3, 0));

        Assert.IsTrue(game.UndoLastMove());
        Assert.IsTrue(game.CanUndo);
        Assert.IsTrue(game.CanRedo);

        Apply(game, new BoardSquare(1, 1), new BoardSquare(3, 1));

        Assert.IsTrue(game.CanUndo);
        Assert.IsFalse(game.CanRedo);
        Assert.HasCount(2, game.MoveHistory);
        Assert.IsNotNull(game.GetPiece(new BoardSquare(1, 0)));
        Assert.IsNull(game.GetPiece(new BoardSquare(3, 0)));
        Assert.IsNull(game.GetPiece(new BoardSquare(1, 1)));
        Assert.IsNotNull(game.GetPiece(new BoardSquare(3, 1)));

        ObservableGameState branchedState = CaptureState(game);
        Assert.IsFalse(game.RedoLastMove());
        AssertState(branchedState, game);
    }

    /// <summary>
    /// Verifies that rejecting an illegal candidate does not discard an
    /// otherwise valid redo branch.
    /// </summary>
    [TestMethod]
    public void RejectedMoveAfterUndoPreservesTheRedoBranch()
    {
        ChessGame game = ChessGame.CreateInitial();
        BoardSquare movedPawnStart = new(10, 0);
        BoardSquare movedPawnEnd = new(8, 0);
        Apply(game, movedPawnStart, movedPawnEnd);
        Assert.IsTrue(game.UndoLastMove());
        ObservableGameState undoneState = CaptureState(game);

        ChessMove illegalMove = new(
            new BoardSquare(10, 1),
            new BoardSquare(7, 1));
        Assert.IsFalse(game.TryMakeMove(
            illegalMove,
            out MoveResult? result));

        Assert.IsNull(result);
        AssertState(undoneState, game);
        Assert.IsFalse(game.CanUndo);
        Assert.IsTrue(game.CanRedo);
        Assert.IsTrue(game.RedoLastMove());
        Assert.IsNull(game.GetPiece(movedPawnStart));
        Assert.IsNotNull(game.GetPiece(movedPawnEnd));
    }

    /// <summary>
    /// Verifies repetition bookkeeping when a draw-producing move is undone,
    /// redone, and then independently applied again.
    /// </summary>
    [TestMethod]
    public void ThreefoldRepetitionSurvivesUndoAndRedo()
    {
        ChessGame game = CreateRepetitionGame();
        RepeatKnightCycle(game);
        RepeatKnightCycle(game);
        ChessMove drawMove = game.MoveHistory[^1].Move;
        ObservableGameState drawnState = CaptureState(game);

        Assert.AreEqual(
            GameStatus.DrawByThreefoldRepetition,
            game.Status);
        Assert.IsTrue(game.IsGameOver);
        Assert.AreEqual(8, game.HalfMoveClock);
        Assert.AreEqual(5, game.FullMoveNumber);

        Assert.IsTrue(game.UndoLastMove());
        ObservableGameState previousState = CaptureState(game);
        Assert.AreEqual(GameStatus.Active, game.Status);
        Assert.IsFalse(game.IsGameOver);
        Assert.AreEqual(7, game.HalfMoveClock);
        Assert.AreEqual(4, game.FullMoveNumber);
        Assert.IsTrue(game.CanRedo);

        Assert.IsTrue(game.RedoLastMove());
        AssertState(drawnState, game);
        Assert.AreEqual(
            GameStatus.DrawByThreefoldRepetition,
            game.Status);

        Assert.IsTrue(game.UndoLastMove());
        AssertState(previousState, game);
        Apply(game, drawMove);
        AssertState(drawnState, game);
        Assert.AreEqual(
            GameStatus.DrawByThreefoldRepetition,
            game.Status);
    }

    /// <summary>
    /// Verifies capture restoration, captured-piece history, and replay.
    /// </summary>
    [TestMethod]
    public void CaptureUndoRedoRestoresBoardAndCapturedPieceHistory()
    {
        BoardSquare rookSquare = new(5, 5);
        BoardSquare knightSquare = new(5, 8);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                rookSquare.Rank,
                rookSquare.File),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                knightSquare.Rank,
                knightSquare.File));
        ChessMove capture = TestGameFactory.FindMove(
            game,
            rookSquare,
            knightSquare);

        RoundTripMove(game, capture);

        Assert.AreEqual(
            PieceKind.Rook,
            game.GetPiece(knightSquare)!.Value.Kind);
        Assert.IsNull(game.GetPiece(rookSquare));
        Assert.HasCount(1, game.GetCapturedPieces(PieceSide.White));
        Assert.AreEqual(
            PieceKind.Knight,
            game.GetCapturedPieces(PieceSide.White)[0].Kind);
    }

    /// <summary>
    /// Verifies that castling replays both piece relocations and the consumed
    /// castling right.
    /// </summary>
    [TestMethod]
    public void CastlingUndoRedoRestoresBothPiecesAndRights()
    {
        BoardSquare kingSquare = new(11, 6);
        BoardSquare rookSquare = new(11, 11);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                kingSquare.Rank,
                kingSquare.File),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                rookSquare.Rank,
                rookSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                6)
        ],
        PieceSide.White,
        CastlingRights.WhiteTowardHigherFile);
        ChessMove castle = game.GetLegalMoves(kingSquare)
            .Single(static move =>
                move.Kind == MoveKind.CastleTowardHigherFile);

        RoundTripMove(game, castle);

        Assert.AreEqual(
            PieceKind.King,
            game.GetPiece(new BoardSquare(11, 8))!.Value.Kind);
        Assert.AreEqual(
            PieceKind.Rook,
            game.GetPiece(new BoardSquare(11, 7))!.Value.Kind);
        Assert.IsNull(game.GetPiece(kingSquare));
        Assert.IsNull(game.GetPiece(rookSquare));
        Assert.AreEqual(CastlingRights.None, game.CastlingRights);
    }

    /// <summary>
    /// Verifies that undoing and replaying en passant preserves its transient
    /// target as well as the off-destination capture.
    /// </summary>
    [TestMethod]
    public void EnPassantUndoRedoRestoresTransientAndCapturedState()
    {
        BoardSquare blackPawnStart = new(4, 5);
        BoardSquare blackPawnEnd = new(6, 5);
        BoardSquare whitePawnStart = new(6, 4);
        BoardSquare enPassantTarget = new(5, 5);
        ChessGame game = CreateEnPassantGame(
            blackPawnStart,
            whitePawnStart);
        ObservableGameState initialState = CaptureState(game);

        Apply(game, blackPawnStart, blackPawnEnd);
        ObservableGameState advancedState = CaptureState(game);
        Assert.AreEqual(enPassantTarget, game.EnPassantTarget);

        ChessMove enPassant = FindEnPassantMove(
            game,
            whitePawnStart,
            enPassantTarget);
        Apply(game, enPassant);
        ObservableGameState capturedState = CaptureState(game);
        Assert.IsNull(game.EnPassantTarget);
        Assert.IsNull(game.GetPiece(blackPawnEnd));
        Assert.HasCount(1, game.GetCapturedPieces(PieceSide.White));

        UndoEnPassantLine(
            game,
            initialState,
            advancedState,
            enPassantTarget,
            blackPawnEnd);
        RedoEnPassantLine(
            game,
            advancedState,
            capturedState);
        Assert.IsNull(game.GetPiece(blackPawnEnd));
        Assert.AreEqual(
            PawnRole.KingPawn,
            game.GetPiece(enPassantTarget)!.Value.PawnRole);
    }

    /// <summary>
    /// Verifies that each custom promotion identity survives undo and redo.
    /// </summary>
    [TestMethod]
    [DataRow(PieceKind.Minister)]
    [DataRow(PieceKind.Chancellor)]
    public void CustomPromotionUndoRedoRestoresTheSelectedPieceIdentity(
        PieceKind promotionKind)
    {
        BoardSquare pawnSquare = new(1, 5);
        BoardSquare promotionSquare = new(0, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                pawnSquare.Rank,
                pawnSquare.File));
        ChessMove promotion = game.GetLegalMoves(pawnSquare)
            .Single(move =>
                move.To == promotionSquare &&
                move.PromotionPiece == promotionKind);

        RoundTripMove(game, promotion);

        BoardPiece promotedPiece =
            game.GetPiece(promotionSquare)!.Value;
        Assert.AreEqual(promotionKind, promotedPiece.Kind);
        Assert.AreEqual(PawnRole.Regular, promotedPiece.PawnRole);
        Assert.IsNull(game.GetPiece(pawnSquare));
    }

    /// <summary>
    /// Verifies atomic Minister exchange fidelity across undo and redo.
    /// </summary>
    [TestMethod]
    public void MinisterExchangeUndoRedoRestoresBothFriendlyPieces()
    {
        BoardSquare ministerSquare = new(7, 2);
        BoardSquare queenSquare = new(4, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                queenSquare.Rank,
                queenSquare.File));
        ChessMove exchange = game.GetLegalMoves(ministerSquare)
            .Single(move =>
                move.To == queenSquare &&
                move.Kind == MoveKind.MinisterExchange);

        RoundTripMove(game, exchange);

        Assert.AreEqual(
            PieceKind.Queen,
            game.GetPiece(ministerSquare)!.Value.Kind);
        Assert.AreEqual(
            PieceKind.Minister,
            game.GetPiece(queenSquare)!.Value.Kind);
        Assert.IsEmpty(game.GetCapturedPieces(PieceSide.White));
    }

    private static void RoundTripMove(
        ChessGame game,
        ChessMove move)
    {
        ObservableGameState beforeMove = CaptureState(game);
        Apply(game, move);
        ObservableGameState afterMove = CaptureState(game);

        Assert.IsTrue(game.CanUndo);
        Assert.IsFalse(game.CanRedo);

        Assert.IsTrue(game.UndoLastMove());
        AssertState(beforeMove, game);
        Assert.IsFalse(game.CanUndo);
        Assert.IsTrue(game.CanRedo);

        Assert.IsTrue(game.RedoLastMove());
        AssertState(afterMove, game);
        Assert.IsTrue(game.CanUndo);
        Assert.IsFalse(game.CanRedo);
    }

    private static void ApplyOpeningLine(ChessGame game)
    {
        Apply(game, new BoardSquare(10, 0), new BoardSquare(8, 0));
        Apply(game, new BoardSquare(1, 0), new BoardSquare(3, 0));
        Apply(game, new BoardSquare(11, 2), new BoardSquare(9, 1));
        Apply(game, new BoardSquare(0, 2), new BoardSquare(2, 1));
    }

    private static void UndoMoves(ChessGame game, int count)
    {
        for (int move = 0; move < count; move++)
        {
            Assert.IsTrue(game.UndoLastMove());
        }
    }

    private static void RedoMoves(ChessGame game, int count)
    {
        for (int move = 0; move < count; move++)
        {
            Assert.IsTrue(game.RedoLastMove());
        }
    }

    private static void AssertUndoBoundary(ChessGame game)
    {
        ObservableGameState boundary = CaptureState(game);

        Assert.IsFalse(game.UndoLastMove());
        AssertState(boundary, game);
        Assert.IsFalse(game.CanUndo);
        Assert.IsTrue(game.CanRedo);
    }

    private static void AssertRedoBoundary(ChessGame game)
    {
        ObservableGameState boundary = CaptureState(game);

        Assert.IsFalse(game.RedoLastMove());
        AssertState(boundary, game);
        Assert.IsTrue(game.CanUndo);
        Assert.IsFalse(game.CanRedo);
    }

    private static ChessGame CreateEnPassantGame(
        BoardSquare blackPawnStart,
        BoardSquare whitePawnStart)
    {
        return TestGameFactory.Create(
            PieceSide.Black,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.Black,
                blackPawnStart.Rank,
                blackPawnStart.File,
                PawnRole.QueenPawn),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                whitePawnStart.Rank,
                whitePawnStart.File,
                PawnRole.KingPawn));
    }

    private static ChessGame CreateRepetitionGame()
    {
        return TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.White,
                10,
                2),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                1,
                9));
    }

    private static void RepeatKnightCycle(ChessGame game)
    {
        Apply(game, new BoardSquare(10, 2), new BoardSquare(8, 1));
        Apply(game, new BoardSquare(1, 9), new BoardSquare(3, 10));
        Apply(game, new BoardSquare(8, 1), new BoardSquare(10, 2));
        Apply(game, new BoardSquare(3, 10), new BoardSquare(1, 9));
    }

    private static ChessMove FindEnPassantMove(
        ChessGame game,
        BoardSquare whitePawn,
        BoardSquare target)
    {
        return game.GetLegalMoves(whitePawn)
            .Single(move =>
                move.To == target &&
                move.Kind == MoveKind.EnPassant);
    }

    private static void UndoEnPassantLine(
        ChessGame game,
        ObservableGameState initialState,
        ObservableGameState advancedState,
        BoardSquare enPassantTarget,
        BoardSquare advancedPawn)
    {
        Assert.IsTrue(game.UndoLastMove());
        AssertState(advancedState, game);
        Assert.AreEqual(enPassantTarget, game.EnPassantTarget);
        Assert.IsNotNull(game.GetPiece(advancedPawn));

        Assert.IsTrue(game.UndoLastMove());
        AssertState(initialState, game);
        Assert.IsFalse(game.CanUndo);
        Assert.IsTrue(game.CanRedo);
    }

    private static void RedoEnPassantLine(
        ChessGame game,
        ObservableGameState advancedState,
        ObservableGameState capturedState)
    {
        Assert.IsTrue(game.RedoLastMove());
        AssertState(advancedState, game);
        Assert.IsTrue(game.RedoLastMove());
        AssertState(capturedState, game);
    }

    private static ObservableGameState CaptureState(ChessGame game)
    {
        return new ObservableGameState(
            [.. game.GetPieces()],
            [.. game.MoveHistory.Select(static result => result.Move)],
            [.. game.GetCapturedPieces(PieceSide.White)],
            [.. game.GetCapturedPieces(PieceSide.Black)],
            game.SideToMove,
            game.Status,
            game.CastlingRights,
            game.EnPassantTarget,
            game.HalfMoveClock,
            game.FullMoveNumber);
    }

    private static void AssertState(
        ObservableGameState expected,
        ChessGame actual)
    {
        Assert.AreSequenceEqual(
            expected.Pieces, [.. actual.GetPieces()]);
        Assert.AreSequenceEqual(
            expected.Moves, [.. actual.MoveHistory.Select(static result => result.Move)]);
        Assert.AreSequenceEqual(
            expected.CapturedByWhite, [.. actual.GetCapturedPieces(PieceSide.White)]);
        Assert.AreSequenceEqual(
            expected.CapturedByBlack, [.. actual.GetCapturedPieces(PieceSide.Black)]);
        Assert.AreEqual(expected.SideToMove, actual.SideToMove);
        Assert.AreEqual(expected.Status, actual.Status);
        Assert.AreEqual(expected.CastlingRights, actual.CastlingRights);
        Assert.AreEqual(expected.EnPassantTarget, actual.EnPassantTarget);
        Assert.AreEqual(expected.HalfMoveClock, actual.HalfMoveClock);
        Assert.AreEqual(expected.FullMoveNumber, actual.FullMoveNumber);
    }

    private static void Apply(
        ChessGame game,
        BoardSquare from,
        BoardSquare to)
    {
        Apply(game, TestGameFactory.FindMove(game, from, to));
    }

    private static void Apply(
        ChessGame game,
        ChessMove move)
    {
        Assert.IsTrue(game.TryMakeMove(move, out MoveResult? result));
        Assert.IsNotNull(result);
    }

    private sealed record ObservableGameState(
        PiecePlacement[] Pieces,
        ChessMove[] Moves,
        BoardPiece[] CapturedByWhite,
        BoardPiece[] CapturedByBlack,
        PieceSide SideToMove,
        GameStatus Status,
        CastlingRights CastlingRights,
        BoardSquare? EnPassantTarget,
        int HalfMoveClock,
        int FullMoveNumber);
}
