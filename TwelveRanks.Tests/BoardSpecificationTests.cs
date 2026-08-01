using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies the fundamental Twelve Ranks board and piece definitions.
/// </summary>
[TestClass]
public sealed class BoardSpecificationTests
{
    /// <summary>
    /// Verifies the board's fixed 12-by-12 dimensions.
    /// </summary>
    [TestMethod]
    public void BoardHasTwelveFilesAndTwelveRanks()
    {
        Assert.AreEqual(12, BoardSpecification.Files);
        Assert.AreEqual(12, BoardSpecification.Ranks);
        Assert.AreEqual(144, BoardSpecification.SquareCount);
    }

    /// <summary>
    /// Verifies that a Royal Guard origin role remains part of piece identity.
    /// </summary>
    [TestMethod]
    public void RoyalGuardOriginRoleIsPartOfThePieceIdentity()
    {
        BoardPiece royalGuard = new(
            PieceKind.Pawn,
            PieceSide.White,
            PawnRole.QueenPawn);

        Assert.AreEqual(PieceKind.Pawn, royalGuard.Kind);
        Assert.AreEqual(PieceSide.White, royalGuard.Side);
        Assert.AreEqual(PawnRole.QueenPawn, royalGuard.PawnRole);
    }
}
