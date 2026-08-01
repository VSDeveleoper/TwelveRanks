using TwelveRanks.Computer;
using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies the public behavior of the conventional computer opponent.
/// </summary>
[TestClass]
public sealed class ConventionalComputerOpponentTests
{
    private static readonly ComputerSearchLimits OnePlyLimits = new(
        TimeSpan.FromSeconds(5),
        maximumDepth: 1,
        maximumQuiescenceDepth: 0);

    /// <summary>
    /// Gets or sets the context supplied by MSTest for the current test.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Verifies that analysis returns an engine-legal move without changing
    /// the authoritative source game.
    /// </summary>
    [TestMethod]
    public void ChoosesLegalMoveWithoutChangingSourceGame()
    {
        ChessGame game = ChessGame.CreateInitial();
        PiecePlacement[] originalPieces = [.. game.GetPieces()];
        ChessMove[] originalLegalMoves = [.. game.GetLegalMoves()];
        ConventionalComputerOpponent opponent = new();

        ComputerMoveDecision decision = opponent.ChooseMove(
            game,
            OnePlyLimits,
            TestContext.CancellationToken);

        Assert.IsTrue(decision.HasMove);
        Assert.IsNotNull(decision.Move);
        Assert.Contains(decision.Move.Value, originalLegalMoves);
        Assert.AreEqual(1, decision.CompletedDepth);
        Assert.IsGreaterThan(0L, decision.NodesSearched);
        Assert.IsFalse(decision.ReachedTimeLimit);
        Assert.IsTrue(originalPieces.SequenceEqual(game.GetPieces()));
        Assert.AreEqual(PieceSide.White, game.SideToMove);
        Assert.AreEqual(GameStatus.Active, game.Status);
        Assert.AreEqual(CastlingRights.All, game.CastlingRights);
        Assert.IsNull(game.EnPassantTarget);
        Assert.AreEqual(0, game.HalfMoveClock);
        Assert.AreEqual(1, game.FullMoveNumber);
        Assert.IsEmpty(game.MoveHistory);
        Assert.IsFalse(game.CanUndo);
    }

    /// <summary>
    /// Verifies that a terminal position produces a decision without a move.
    /// </summary>
    [TestMethod]
    public void CheckmatedPositionReturnsDecisionWithoutMove()
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                0),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                1,
                1),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                2,
                2)
        ],
        PieceSide.Black);
        ConventionalComputerOpponent opponent = new();

        ComputerMoveDecision decision = opponent.ChooseMove(
            game,
            OnePlyLimits,
            TestContext.CancellationToken);

        Assert.AreEqual(GameStatus.Checkmate, game.Status);
        Assert.IsFalse(decision.HasMove);
        Assert.IsNull(decision.Move);
        Assert.AreEqual(0, decision.CompletedDepth);
        Assert.AreEqual(0L, decision.NodesSearched);
        Assert.IsFalse(decision.ReachedTimeLimit);
    }

    /// <summary>
    /// Verifies that the opponent recognizes an immediate checkmate.
    /// </summary>
    [TestMethod]
    public void ChoosesMoveThatDeliversMateInOne()
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                0),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                2,
                2),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                2,
                1)
        ]);
        ConventionalComputerOpponent opponent = new();

        ComputerMoveDecision decision = opponent.ChooseMove(
            game,
            OnePlyLimits,
            TestContext.CancellationToken);

        Assert.IsNotNull(decision.Move);
        ChessGame result = game.Clone();
        Assert.IsTrue(result.TryMakeMove(
            decision.Move.Value,
            out MoveResult? moveResult));
        Assert.IsNotNull(moveResult);
        Assert.AreEqual(GameStatus.Checkmate, result.Status);
        Assert.AreEqual(PieceSide.White, result.Winner);
    }

    /// <summary>
    /// Verifies that a clearly advantageous standard capture is selected.
    /// </summary>
    [TestMethod]
    public void ChoosesClearlyAdvantageousQueenCapture()
    {
        BoardSquare rookSquare = new(5, 5);
        BoardSquare queenSquare = new(5, 8);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                rookSquare.Rank,
                rookSquare.File),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.Black,
                queenSquare.Rank,
                queenSquare.File));
        ConventionalComputerOpponent opponent = new();

        ComputerMoveDecision decision = opponent.ChooseMove(
            game,
            OnePlyLimits,
            TestContext.CancellationToken);

        Assert.AreEqual(
            new ChessMove(rookSquare, queenSquare, MoveKind.Capture),
            decision.Move!.Value);
    }

    /// <summary>
    /// Verifies that every Twelve Ranks custom movement category participates
    /// in the same legal-move search as the standard pieces.
    /// </summary>
    /// <param name="pieceKind">The custom piece category.</param>
    /// <param name="pawnRole">The pawn's Royal Guard identity, when applicable.</param>
    /// <param name="fromRank">The moving piece's rank.</param>
    /// <param name="fromFile">The moving piece's file.</param>
    /// <param name="toRank">The capturable Queen's rank.</param>
    /// <param name="toFile">The capturable Queen's file.</param>
    [TestMethod]
    [DataRow(PieceKind.Minister, PawnRole.Regular, 5, 5, 3, 3)]
    [DataRow(PieceKind.Chancellor, PawnRole.Regular, 5, 5, 5, 8)]
    [DataRow(PieceKind.Pawn, PawnRole.QueenPawn, 7, 5, 6, 4)]
    public void CustomPieceChoosesAvailableQueenCapture(
        PieceKind pieceKind,
        PawnRole pawnRole,
        int fromRank,
        int fromFile,
        int toRank,
        int toFile)
    {
        BoardSquare from = new(fromRank, fromFile);
        BoardSquare to = new(toRank, toFile);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                pieceKind,
                PieceSide.White,
                from.Rank,
                from.File,
                pawnRole),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.Black,
                to.Rank,
                to.File));
        ConventionalComputerOpponent opponent = new();

        ComputerMoveDecision decision = opponent.ChooseMove(
            game,
            OnePlyLimits,
            TestContext.CancellationToken);

        Assert.AreEqual(
            new ChessMove(from, to, MoveKind.Capture),
            decision.Move!.Value);
    }

    /// <summary>
    /// Verifies that a supplied seed keeps opening selection reproducible.
    /// </summary>
    [TestMethod]
    public void SeededOpeningSearchesAreReproducible()
    {
        ChessGame game = CreatePositionAfterWhiteMove(
            new BoardSquare(10, 0),
            new BoardSquare(8, 0));
        ConventionalComputerOpponent firstOpponent = new(1729);
        ConventionalComputerOpponent secondOpponent = new(1729);

        ChessMove?[] firstSequence =
        [
            firstOpponent.ChooseMove(
                game,
                OnePlyLimits,
                TestContext.CancellationToken).Move,
            firstOpponent.ChooseMove(
                game,
                OnePlyLimits,
                TestContext.CancellationToken).Move,
            firstOpponent.ChooseMove(
                game,
                OnePlyLimits,
                TestContext.CancellationToken).Move
        ];
        ChessMove?[] secondSequence =
        [
            secondOpponent.ChooseMove(
                game,
                OnePlyLimits,
                TestContext.CancellationToken).Move,
            secondOpponent.ChooseMove(
                game,
                OnePlyLimits,
                TestContext.CancellationToken).Move,
            secondOpponent.ChooseMove(
                game,
                OnePlyLimits,
                TestContext.CancellationToken).Move
        ];

        Assert.IsTrue(firstSequence.SequenceEqual(secondSequence));
    }

    /// <summary>
    /// Verifies that near-equivalent opening moves are not permanently resolved
    /// by board-coordinate order.
    /// </summary>
    [TestMethod]
    public void OpeningSearchCanChooseDifferentNearEquivalentMoves()
    {
        ChessGame game = CreatePositionAfterWhiteMove(
            new BoardSquare(10, 3),
            new BoardSquare(8, 3));
        ChessMove[] legalMoves = [.. game.GetLegalMoves()];
        HashSet<ChessMove> selectedMoves = [];

        for (int seed = 0; seed < 16; seed++)
        {
            ConventionalComputerOpponent opponent = new(seed);
            ComputerMoveDecision decision = opponent.ChooseMove(
                game,
                OnePlyLimits,
                TestContext.CancellationToken);

            Assert.IsNotNull(decision.Move);
            Assert.Contains(decision.Move.Value, legalMoves);
            _ = selectedMoves.Add(decision.Move.Value);
        }

        Assert.IsGreaterThan(1, selectedMoves.Count);
    }

    /// <summary>
    /// Verifies cancellation at the public search boundary.
    /// </summary>
    [TestMethod]
    public void PreCanceledSearchThrowsOperationCanceledException()
    {
        ChessGame game = ChessGame.CreateInitial();
        ConventionalComputerOpponent opponent = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        _ = Assert.ThrowsExactly<OperationCanceledException>(
            () => opponent.ChooseMove(
                game,
                OnePlyLimits,
                cancellation.Token));
    }

    /// <summary>
    /// Verifies that an expired search still returns its deterministic legal
    /// fallback rather than losing the turn.
    /// </summary>
    [TestMethod]
    public void ImmediateDeadlineReturnsLegalFallbackMove()
    {
        ChessGame game = ChessGame.CreateInitial();
        ChessMove[] legalMoves = [.. game.GetLegalMoves()];
        ComputerSearchLimits immediateLimits = new(
            TimeSpan.FromTicks(1),
            maximumDepth: 6,
            maximumQuiescenceDepth: 2);
        ConventionalComputerOpponent opponent = new();

        ComputerMoveDecision decision = opponent.ChooseMove(
            game,
            immediateLimits,
            TestContext.CancellationToken);

        Assert.IsNotNull(decision.Move);
        Assert.Contains(decision.Move.Value, legalMoves);
        Assert.AreEqual(0, decision.CompletedDepth);
        Assert.IsTrue(decision.ReachedTimeLimit);
    }

    /// <summary>
    /// Verifies null argument validation at the opponent boundary.
    /// </summary>
    [TestMethod]
    public void OpponentRejectsNullArguments()
    {
        ConventionalComputerOpponent opponent = new();
        ChessGame game = ChessGame.CreateInitial();

        _ = Assert.ThrowsExactly<ArgumentNullException>(
            () => opponent.ChooseMove(null!, OnePlyLimits, TestContext.CancellationToken));
        _ = Assert.ThrowsExactly<ArgumentNullException>(
            () => opponent.ChooseMove(game, null!, TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies invalid time and depth bounds at the limits boundary.
    /// </summary>
    [TestMethod]
    public void SearchLimitsRejectInvalidBounds()
    {
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => new ComputerSearchLimits(
                TimeSpan.Zero,
                maximumDepth: 1,
                maximumQuiescenceDepth: 0));
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => new ComputerSearchLimits(
                TimeSpan.FromSeconds(1),
                maximumDepth: 0,
                maximumQuiescenceDepth: 0));
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => new ComputerSearchLimits(
                TimeSpan.FromSeconds(1),
                maximumDepth: 1,
                maximumQuiescenceDepth: -1));
    }

    private static ChessGame CreatePositionAfterWhiteMove(
        BoardSquare from,
        BoardSquare to)
    {
        ChessGame game = ChessGame.CreateInitial();
        ChessMove move = game.GetLegalMoves(from)
            .Single(candidate => candidate.To == to);

        Assert.IsTrue(game.TryMakeMove(move, out _));
        Assert.AreEqual(PieceSide.Black, game.SideToMove);

        return game;
    }
}
