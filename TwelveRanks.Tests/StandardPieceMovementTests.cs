using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies ordinary piece movement generalized to a 12-by-12 board.
/// </summary>
[TestClass]
public sealed class StandardPieceMovementTests
{
    /// <summary>
    /// Verifies central rook movement across the wider board.
    /// </summary>
    [TestMethod]
    public void CentralRookHasTwentyTwoEmptyBoardMoves()
    {
        BoardSquare from = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                from.Rank,
                from.File));

        Assert.HasCount(22, game.GetLegalMoves(from));
    }

    /// <summary>
    /// Verifies central bishop movement across all four diagonals.
    /// </summary>
    [TestMethod]
    public void CentralBishopHasTwentyOneEmptyBoardMoves()
    {
        BoardSquare from = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Bishop,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.Black,
                1,
                0));

        Assert.HasCount(21, game.GetLegalMoves(from));
    }

    /// <summary>
    /// Verifies combined rook and bishop movement for the queen.
    /// </summary>
    [TestMethod]
    public void CentralQueenHasFortyThreeEmptyBoardMoves()
    {
        BoardSquare from = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                from.Rank,
                from.File));

        Assert.HasCount(43, game.GetLegalMoves(from));
    }

    /// <summary>
    /// Verifies the standard eight central knight jumps.
    /// </summary>
    [TestMethod]
    public void CentralKnightHasEightMoves()
    {
        BoardSquare from = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.Black,
                1,
                0));

        Assert.HasCount(8, game.GetLegalMoves(from));
    }

    /// <summary>
    /// Verifies that sliding pieces stop at the first occupied square.
    /// </summary>
    [TestMethod]
    public void RookStopsAtFriendlyBlockerAndMayCaptureEnemyBlocker()
    {
        BoardSquare from = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                5,
                3),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.Black,
                5,
                8));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, from);

        Assert.DoesNotContain(new BoardSquare(5, 3), destinations);
        Assert.DoesNotContain(new BoardSquare(5, 2), destinations);
        Assert.Contains(new BoardSquare(5, 8), destinations);
        Assert.DoesNotContain(new BoardSquare(5, 9), destinations);
    }

    /// <summary>
    /// Verifies turn enforcement and authoritative capture application.
    /// </summary>
    [TestMethod]
    public void SuccessfulCaptureRemovesPieceAndChangesTurn()
    {
        BoardSquare from = new(5, 5);
        BoardSquare target = new(5, 8);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                target.Rank,
                target.File));
        ChessMove move = TestGameFactory.FindMove(game, from, target);

        bool applied = game.TryMakeMove(move, out MoveResult? result);

        Assert.IsTrue(applied);
        Assert.IsNotNull(result);
        Assert.AreEqual(PieceKind.Knight, result.CapturedPiece!.Value.Kind);
        Assert.IsNull(game.GetPiece(from));
        Assert.AreEqual(PieceKind.Rook, game.GetPiece(target)!.Value.Kind);
        Assert.AreEqual(PieceSide.Black, game.SideToMove);
        Assert.HasCount(1, game.GetCapturedPieces(PieceSide.White));
    }
}
