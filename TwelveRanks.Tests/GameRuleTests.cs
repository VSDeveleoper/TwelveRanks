using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies king safety, game outcomes, castling, repetition, and undo.
/// </summary>
[TestClass]
public sealed class GameRuleTests
{
    /// <summary>
    /// Verifies that a pinned piece cannot expose its king.
    /// </summary>
    [TestMethod]
    public void PinnedRookCannotMoveOffTheCheckingFile()
    {
        BoardSquare rookSquare = new(10, 0);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                rookSquare.Rank,
                rookSquare.File),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                0,
                0));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, rookSquare);

        Assert.DoesNotContain(new BoardSquare(10, 1), destinations);
        Assert.Contains(new BoardSquare(9, 0), destinations);
    }

    /// <summary>
    /// Verifies that opposing kings cannot become adjacent.
    /// </summary>
    [TestMethod]
    public void KingCannotMoveNextToTheOpposingKing()
    {
        BoardSquare whiteKing = new(5, 5);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                whiteKing.Rank,
                whiteKing.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                3,
                5),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                11,
                11)
        ]);

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, whiteKing);

        Assert.DoesNotContain(new BoardSquare(4, 5), destinations);
    }

    /// <summary>
    /// Verifies that checkmate is detected without capturing the king.
    /// </summary>
    [TestMethod]
    public void CornerCheckmateIsDetected()
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

        Assert.AreEqual(GameStatus.Checkmate, game.Status);
        Assert.AreEqual(PieceSide.White, game.Winner);
        Assert.IsTrue(game.IsGameOver);
        Assert.IsEmpty(game.GetLegalMoves());
    }

    /// <summary>
    /// Verifies that stalemate is distinguished from checkmate.
    /// </summary>
    [TestMethod]
    public void CornerStalemateIsDetected()
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
                2),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                2,
                1)
        ],
        PieceSide.Black);

        Assert.AreEqual(GameStatus.Stalemate, game.Status);
        Assert.IsNull(game.Winner);
        Assert.IsTrue(game.IsGameOver);
    }

    /// <summary>
    /// Verifies that legal moves never capture the opposing king.
    /// </summary>
    [TestMethod]
    public void OpposingKingIsNeverACaptureDestination()
    {
        BoardSquare rookSquare = new(5, 5);
        BoardSquare blackKing = new(5, 8);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                11,
                0),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                blackKing.Rank,
                blackKing.File),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                rookSquare.Rank,
                rookSquare.File)
        ]);

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, rookSquare);

        Assert.DoesNotContain(blackKing, destinations);
    }

    /// <summary>
    /// Verifies both generalized castling destinations on a clear board.
    /// </summary>
    [TestMethod]
    public void KingMayCastleTwoSquaresTowardEitherOriginalRook()
    {
        BoardSquare kingSquare = new(11, 6);
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
                11,
                0),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                11,
                11),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                6)
        ],
        PieceSide.White,
        CastlingRights.All);

        ChessMove[] castles = [.. game.GetLegalMoves(kingSquare)
            .Where(static move => move.Kind is
                MoveKind.CastleTowardLowerFile or
                MoveKind.CastleTowardHigherFile)];

        Assert.HasCount(2, castles);
        Assert.Contains(static move => move.To == new BoardSquare(11, 4), castles);
        Assert.Contains(static move => move.To == new BoardSquare(11, 8), castles);
    }

    /// <summary>
    /// Verifies the final king and rook squares after castling.
    /// </summary>
    [TestMethod]
    public void HigherFileCastlingMovesBothKingAndRook()
    {
        BoardSquare kingSquare = new(11, 6);
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
                11,
                11),
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

        Assert.IsTrue(game.TryMakeMove(castle, out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.AreEqual(
            PieceKind.King,
            game.GetPiece(new BoardSquare(11, 8))!.Value.Kind);
        Assert.AreEqual(
            PieceKind.Rook,
            game.GetPiece(new BoardSquare(11, 7))!.Value.Kind);
        Assert.IsNull(game.GetPiece(new BoardSquare(11, 6)));
        Assert.IsNull(game.GetPiece(new BoardSquare(11, 11)));
        Assert.AreEqual(CastlingRights.None, game.CastlingRights);
    }

    /// <summary>
    /// Verifies that castling through an attacked square is forbidden.
    /// </summary>
    [TestMethod]
    public void KingCannotCastleThroughCheck()
    {
        BoardSquare kingSquare = new(11, 6);
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
                11,
                11),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                6),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                0,
                7)
        ],
        PieceSide.White,
        CastlingRights.WhiteTowardHigherFile);

        Assert.DoesNotContain(static move =>
                move.Kind == MoveKind.CastleTowardHigherFile, game.GetLegalMoves(kingSquare));
    }

    /// <summary>
    /// Verifies that moving an original rook permanently clears its right.
    /// </summary>
    [TestMethod]
    public void MovingRookClearsOnlyItsCastlingRight()
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                11,
                6),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                11,
                0),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                11,
                11),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                6)
        ],
        PieceSide.White,
        CastlingRights.All);

        Apply(game, new BoardSquare(11, 11), new BoardSquare(10, 11));

        Assert.IsFalse(game.CastlingRights.HasFlag(
            CastlingRights.WhiteTowardHigherFile));
        Assert.IsTrue(game.CastlingRights.HasFlag(
            CastlingRights.WhiteTowardLowerFile));
        Assert.IsTrue(game.CastlingRights.HasFlag(
            CastlingRights.BlackTowardLowerFile));
    }

    /// <summary>
    /// Verifies exact restoration of position state through undo.
    /// </summary>
    [TestMethod]
    public void UndoRestoresBoardTurnRightsAndHistory()
    {
        ChessGame game = ChessGame.CreateInitial();
        BoardSquare from = new(10, 0);
        BoardSquare to = new(8, 0);
        CastlingRights originalRights = game.CastlingRights;

        Apply(game, from, to);

        Assert.IsTrue(game.CanUndo);
        Assert.IsTrue(game.UndoLastMove());
        Assert.AreEqual(PieceSide.White, game.SideToMove);
        Assert.AreEqual(originalRights, game.CastlingRights);
        Assert.IsNotNull(game.GetPiece(from));
        Assert.IsNull(game.GetPiece(to));
        Assert.IsEmpty(game.MoveHistory);
        Assert.IsFalse(game.CanUndo);
    }

    /// <summary>
    /// Verifies threefold repetition using reversible knight moves.
    /// </summary>
    [TestMethod]
    public void ThirdPositionOccurrenceProducesARepetitionDraw()
    {
        ChessGame game = TestGameFactory.Create(
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

        RepeatKnightCycle(game);
        RepeatKnightCycle(game);

        Assert.AreEqual(
            GameStatus.DrawByThreefoldRepetition,
            game.Status);
        Assert.IsTrue(game.IsGameOver);
    }

    /// <summary>
    /// Verifies that an unusable en-passant target does not split repetitions.
    /// </summary>
    [TestMethod]
    public void NonCapturableEnPassantTargetDoesNotSplitRepetition()
    {
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                8,
                5,
                PawnRole.QueenPawn),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                0,
                8));

        Apply(game, new BoardSquare(8, 5), new BoardSquare(6, 5));
        RepeatIrrelevantEnPassantCycle(game);
        RepeatIrrelevantEnPassantCycle(game);

        Assert.AreEqual(
            GameStatus.DrawByThreefoldRepetition,
            game.Status);
    }

    /// <summary>
    /// Verifies that cloned games can be searched without changing the source.
    /// </summary>
    [TestMethod]
    public void CloneMovesIndependentlyFromSourceGame()
    {
        ChessGame source = ChessGame.CreateInitial();
        ChessGame clone = source.Clone();

        Apply(clone, new BoardSquare(10, 0), new BoardSquare(8, 0));

        Assert.IsNotNull(source.GetPiece(new BoardSquare(10, 0)));
        Assert.IsNull(source.GetPiece(new BoardSquare(8, 0)));
        Assert.AreEqual(PieceSide.White, source.SideToMove);
        Assert.AreEqual(PieceSide.Black, clone.SideToMove);
    }

    private static void RepeatKnightCycle(ChessGame game)
    {
        Apply(game, new BoardSquare(10, 2), new BoardSquare(8, 1));
        Apply(game, new BoardSquare(1, 9), new BoardSquare(3, 10));
        Apply(game, new BoardSquare(8, 1), new BoardSquare(10, 2));
        Apply(game, new BoardSquare(3, 10), new BoardSquare(1, 9));
    }

    private static void RepeatIrrelevantEnPassantCycle(ChessGame game)
    {
        Apply(game, new BoardSquare(0, 8), new BoardSquare(2, 9));
        Apply(game, new BoardSquare(6, 5), new BoardSquare(7, 5));
        Apply(game, new BoardSquare(2, 9), new BoardSquare(0, 8));
        Apply(game, new BoardSquare(7, 5), new BoardSquare(6, 5));
    }

    private static void Apply(
        ChessGame game,
        BoardSquare from,
        BoardSquare to)
    {
        ChessMove move = TestGameFactory.FindMove(game, from, to);
        Assert.IsTrue(game.TryMakeMove(move, out MoveResult? result));
        Assert.IsNotNull(result);
    }
}
