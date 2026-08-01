using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies the Chancellor's combined rook and knight movement.
/// </summary>
[TestClass]
public sealed class ChancellorMovementTests
{
    /// <summary>
    /// Verifies the complete central rook-and-knight move union.
    /// </summary>
    [TestMethod]
    public void CentralChancellorHasThirtyRookAndKnightMovesOnly()
    {
        BoardSquare from = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Chancellor,
                PieceSide.White,
                from.Rank,
                from.File));

        IReadOnlyList<ChessMove> moves = game.GetLegalMoves(from);
        HashSet<BoardSquare> destinations =
            [.. moves.Select(static move => move.To)];

        Assert.HasCount(30, moves);
        Assert.Contains(new BoardSquare(5, 0), destinations);
        Assert.Contains(new BoardSquare(3, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(4, 4), destinations);
        Assert.AreEqual(GameStatus.Active, game.Status);
        Assert.IsFalse(game.IsGameOver);
    }

    /// <summary>
    /// Verifies rook-ray blocking and knight landing-square behavior.
    /// </summary>
    [TestMethod]
    public void RookRayStopsAtBlockersWhileKnightJumpAndLandingRulesRemainCorrect()
    {
        BoardSquare from = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Chancellor,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                5,
                3),
            TestGameFactory.Place(
                PieceKind.Bishop,
                PieceSide.Black,
                5,
                8),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                4,
                5),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                3,
                6),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                7,
                6));

        IReadOnlyList<ChessMove> moves = game.GetLegalMoves(from);
        HashSet<BoardSquare> destinations =
            [.. moves.Select(static move => move.To)];

        Assert.DoesNotContain(new BoardSquare(5, 3), destinations);
        Assert.DoesNotContain(new BoardSquare(5, 2), destinations);
        Assert.AreEqual(
            MoveKind.Capture,
            moves.Single(move => move.To == new BoardSquare(5, 8)).Kind);
        Assert.DoesNotContain(new BoardSquare(5, 9), destinations);
        Assert.AreEqual(
            MoveKind.Quiet,
            moves.Single(move => move.To == new BoardSquare(3, 4)).Kind);
        Assert.DoesNotContain(new BoardSquare(3, 6), destinations);
        Assert.AreEqual(
            MoveKind.Capture,
            moves.Single(move => move.To == new BoardSquare(7, 6)).Kind);
    }

    /// <summary>
    /// Verifies that both Chancellor movement patterns contribute to check.
    /// </summary>
    /// <param name="chancellorRank">The Chancellor's rank.</param>
    /// <param name="chancellorFile">The Chancellor's file.</param>
    [TestMethod]
    [DataRow(4, 1)]
    [DataRow(6, 4)]
    public void ChancellorGivesCheckByRookAndKnightReach(
        int chancellorRank,
        int chancellorFile)
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                11,
                0),
            TestGameFactory.Place(
                PieceKind.Chancellor,
                PieceSide.White,
                chancellorRank,
                chancellorFile),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                4,
                5)
        ],
        PieceSide.Black);

        Assert.IsTrue(game.IsInCheck(PieceSide.Black));
        Assert.AreEqual(GameStatus.Check, game.Status);
    }

    /// <summary>
    /// Verifies that a blocker interrupts the Chancellor's rook attack map.
    /// </summary>
    [TestMethod]
    public void BlockedChancellorRookRayDoesNotGiveCheck()
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                11,
                0),
            TestGameFactory.Place(
                PieceKind.Chancellor,
                PieceSide.White,
                4,
                1),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                4,
                3),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                4,
                5)
        ],
        PieceSide.Black);

        Assert.IsFalse(game.IsInCheck(PieceSide.Black));
        Assert.AreEqual(GameStatus.Active, game.Status);
    }

    /// <summary>
    /// Verifies that compound moves cannot expose the moving side's king.
    /// </summary>
    [TestMethod]
    public void PinnedChancellorCannotMoveOffTheProtectingFile()
    {
        BoardSquare chancellorSquare = new(10, 5);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                11,
                5),
            TestGameFactory.Place(
                PieceKind.Chancellor,
                PieceSide.White,
                chancellorSquare.Rank,
                chancellorSquare.File),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                0,
                5),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                11)
        ]);

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, chancellorSquare);

        Assert.Contains(new BoardSquare(9, 5), destinations);
        Assert.DoesNotContain(new BoardSquare(10, 6), destinations);
        Assert.DoesNotContain(new BoardSquare(8, 4), destinations);
    }

    /// <summary>
    /// Verifies Chancellor capture application and complete undo restoration.
    /// </summary>
    [TestMethod]
    public void ChancellorCaptureAndUndoRestoreBothPiecesAndState()
    {
        BoardSquare from = new(5, 5);
        BoardSquare target = new(5, 8);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Chancellor,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                target.Rank,
                target.File));
        ChessMove capture = TestGameFactory.FindMove(game, from, target);

        Assert.AreEqual(MoveKind.Capture, capture.Kind);
        Assert.IsTrue(game.TryMakeMove(capture, out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.AreEqual(PieceKind.Chancellor, result.MovingPiece.Kind);
        Assert.AreEqual(PieceKind.Knight, result.CapturedPiece!.Value.Kind);
        Assert.IsNull(game.GetPiece(from));
        Assert.AreEqual(
            PieceKind.Chancellor,
            game.GetPiece(target)!.Value.Kind);
        Assert.AreEqual(PieceSide.Black, game.SideToMove);
        Assert.HasCount(1, game.MoveHistory);
        Assert.HasCount(1, game.GetCapturedPieces(PieceSide.White));

        Assert.IsTrue(game.UndoLastMove());
        Assert.AreEqual(
            PieceKind.Chancellor,
            game.GetPiece(from)!.Value.Kind);
        Assert.AreEqual(PieceKind.Knight, game.GetPiece(target)!.Value.Kind);
        Assert.AreEqual(PieceSide.White, game.SideToMove);
        Assert.AreEqual(GameStatus.Active, game.Status);
        Assert.IsEmpty(game.MoveHistory);
        Assert.IsEmpty(game.GetCapturedPieces(PieceSide.White));
        Assert.IsFalse(game.CanUndo);
    }
}
