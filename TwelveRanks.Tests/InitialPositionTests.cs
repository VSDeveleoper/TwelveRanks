using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies the authoritative Core-owned starting position.
/// </summary>
[TestClass]
public sealed class InitialPositionTests
{
    /// <summary>
    /// Verifies the piece count and side to move.
    /// </summary>
    [TestMethod]
    public void InitialGameContainsFortyEightPiecesAndWhiteMovesFirst()
    {
        ChessGame game = ChessGame.CreateInitial();

        Assert.HasCount(48, game.GetPieces());
        Assert.AreEqual(PieceSide.White, game.SideToMove);
        Assert.AreEqual(GameStatus.Active, game.Status);
    }

    /// <summary>
    /// Verifies the Chancellors, Ministers, Royal Guards, and filled back ranks.
    /// </summary>
    [TestMethod]
    public void InitialGamePlacesAllCustomPiecesAndRoyalGuards()
    {
        ChessGame game = ChessGame.CreateInitial();

        foreach (int rank in new[] { 0, 11 })
        {
            PieceSide expectedSide = rank == 0
                ? PieceSide.Black
                : PieceSide.White;

            for (int file = 0; file < BoardSpecification.Files; file++)
            {
                Assert.IsNotNull(game.GetPiece(new BoardSquare(rank, file)));
            }

            foreach (int file in new[] { 1, 10 })
            {
                BoardPiece chancellor =
                    game.GetPiece(new BoardSquare(rank, file))!.Value;

                Assert.AreEqual(PieceKind.Chancellor, chancellor.Kind);
                Assert.AreEqual(expectedSide, chancellor.Side);
            }

            foreach (int file in new[] { 4, 7 })
            {
                BoardPiece minister =
                    game.GetPiece(new BoardSquare(rank, file))!.Value;

                Assert.AreEqual(PieceKind.Minister, minister.Kind);
                Assert.AreEqual(expectedSide, minister.Side);
            }
        }

        Assert.AreEqual(
            PawnRole.QueenPawn,
            game.GetPiece(new BoardSquare(10, 5))!.Value.PawnRole);
        Assert.AreEqual(
            PawnRole.KingPawn,
            game.GetPiece(new BoardSquare(10, 6))!.Value.PawnRole);
        Assert.AreEqual(
            PawnRole.QueenPawn,
            game.GetPiece(new BoardSquare(1, 5))!.Value.PawnRole);
        Assert.AreEqual(
            PawnRole.KingPawn,
            game.GetPiece(new BoardSquare(1, 6))!.Value.PawnRole);
    }

    /// <summary>
    /// Verifies coordinate notation at all four corners.
    /// </summary>
    [TestMethod]
    public void BoardSquareNotationUsesFilesAThroughLAndRanksOneThroughTwelve()
    {
        Assert.AreEqual("a12", new BoardSquare(0, 0).Notation);
        Assert.AreEqual("l12", new BoardSquare(0, 11).Notation);
        Assert.AreEqual("a1", new BoardSquare(11, 0).Notation);
        Assert.AreEqual("l1", new BoardSquare(11, 11).Notation);
    }

    /// <summary>
    /// Verifies that Royal Guard roles cannot be attached to non-pawns.
    /// </summary>
    [TestMethod]
    public void NonPawnCannotCarryARoyalGuardRole()
    {
        _ = Assert.ThrowsExactly<ArgumentException>(
            static () => new BoardPiece(
                PieceKind.Rook,
                PieceSide.White,
                PawnRole.QueenPawn));
    }

    /// <summary>
    /// Verifies that undefined serialized piece values fail at the Core boundary.
    /// </summary>
    [TestMethod]
    public void BoardPieceRejectsUndefinedEnumValues()
    {
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => new BoardPiece(
                (PieceKind)99,
                PieceSide.White));
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => new BoardPiece(
                PieceKind.Pawn,
                (PieceSide)99));
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => new BoardPiece(
                PieceKind.Pawn,
                PieceSide.White,
                (PawnRole)99));
    }
}
