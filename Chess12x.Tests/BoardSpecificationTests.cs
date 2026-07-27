using Chess12x.Core;

namespace Chess12x.Tests;

[TestClass]
public sealed class BoardSpecificationTests
{
    [TestMethod]
    public void Board_HasTwelveFilesAndTwelveRanks()
    {
        Assert.AreEqual(12, BoardSpecification.Files);
        Assert.AreEqual(12, BoardSpecification.Ranks);
        Assert.AreEqual(144, BoardSpecification.SquareCount);
    }
}
