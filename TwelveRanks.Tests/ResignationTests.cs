using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies resignation results and their effect on further play.
/// </summary>
[TestClass]
public sealed class ResignationTests
{
    /// <summary>
    /// Verifies that either side can resign and awards the game to the
    /// opponent.
    /// </summary>
    [TestMethod]
    [DataRow(PieceSide.White, PieceSide.Black)]
    [DataRow(PieceSide.Black, PieceSide.White)]
    public void ResignationEndsGameAndAwardsOpponent(
        PieceSide resignedSide,
        PieceSide expectedWinner)
    {
        ChessGame game = ChessGame.CreateInitial();
        ChessMove legalMove = game.GetLegalMoves()[0];

        Assert.IsTrue(game.Resign(resignedSide));
        Assert.IsTrue(game.IsGameOver);
        Assert.AreEqual(resignedSide, game.ResignedSide);
        Assert.AreEqual(expectedWinner, game.Winner);
        Assert.HasCount(0, game.GetLegalMoves());
        Assert.IsFalse(game.TryMakeMove(legalMove, out _));
    }

    /// <summary>
    /// Verifies that an already completed game cannot be resigned again.
    /// </summary>
    [TestMethod]
    public void CompletedGameRejectsAnotherResignation()
    {
        ChessGame game = ChessGame.CreateInitial();

        Assert.IsTrue(game.Resign(PieceSide.White));
        Assert.IsFalse(game.Resign(PieceSide.Black));
        Assert.AreEqual(PieceSide.White, game.ResignedSide);
        Assert.AreEqual(PieceSide.Black, game.Winner);
    }

    /// <summary>
    /// Verifies that resignation is final and cannot be bypassed through
    /// move-history commands.
    /// </summary>
    [TestMethod]
    public void ResignationDisablesUndoAndRedo()
    {
        ChessGame game = ChessGame.CreateInitial();
        ChessMove openingMove = game.GetLegalMoves()[0];
        Assert.IsTrue(game.TryMakeMove(openingMove, out _));
        Assert.IsTrue(game.CanUndo);

        Assert.IsTrue(game.Resign(PieceSide.Black));

        Assert.IsFalse(game.CanUndo);
        Assert.IsFalse(game.CanRedo);
        Assert.IsFalse(game.UndoLastMove());
        Assert.IsFalse(game.RedoLastMove());
    }

    /// <summary>
    /// Verifies that analysis snapshots retain resignation outcomes.
    /// </summary>
    [TestMethod]
    public void ClonePreservesResignationOutcome()
    {
        ChessGame game = ChessGame.CreateInitial();
        Assert.IsTrue(game.Resign(PieceSide.Black));

        ChessGame clone = game.Clone();

        Assert.IsTrue(clone.IsGameOver);
        Assert.AreEqual(PieceSide.Black, clone.ResignedSide);
        Assert.AreEqual(PieceSide.White, clone.Winner);
    }
}
