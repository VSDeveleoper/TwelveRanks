using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies the Minister's compound movement and royal exchange.
/// </summary>
[TestClass]
public sealed class MinisterMovementTests
{
    /// <summary>
    /// Verifies that a central Minister combines bishop and knight movement.
    /// </summary>
    [TestMethod]
    public void CentralMinisterHasTwentyNineEmptyBoardMoves()
    {
        BoardSquare ministerSquare = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File));

        Assert.HasCount(29, game.GetLegalMoves(ministerSquare));
    }

    /// <summary>
    /// Verifies bishop blocking without interfering with knight jumps.
    /// </summary>
    [TestMethod]
    public void BishopRayStopsAtBlockerWhileKnightJumpRemainsAvailable()
    {
        BoardSquare ministerSquare = new(5, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                4,
                4),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                3,
                3),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                7,
                7));

        IReadOnlyList<ChessMove> moves =
            game.GetLegalMoves(ministerSquare);
        HashSet<BoardSquare> destinations =
            [.. moves.Select(static move => move.To)];

        Assert.DoesNotContain(new BoardSquare(4, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(3, 3), destinations);
        Assert.Contains(new BoardSquare(7, 7), destinations);
        Assert.DoesNotContain(new BoardSquare(8, 8), destinations);
        Assert.Contains(new BoardSquare(3, 4), destinations);
        Assert.DoesNotContain(static move =>
            move.Kind == MoveKind.MinisterExchange, moves);
    }

    /// <summary>
    /// Verifies that enemies are captured and non-royal allies block movement.
    /// </summary>
    [TestMethod]
    public void EnemyQueenIsCapturedButFriendlyRookCannotBeExchanged()
    {
        BoardSquare ministerSquare = new(5, 5);
        BoardSquare enemyQueenSquare = new(3, 3);
        BoardSquare friendlyRookSquare = new(7, 7);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.Black,
                enemyQueenSquare.Rank,
                enemyQueenSquare.File),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                friendlyRookSquare.Rank,
                friendlyRookSquare.File));

        ChessMove capture = game.GetLegalMoves(ministerSquare)
            .Single(move => move.To == enemyQueenSquare);

        Assert.AreEqual(MoveKind.Capture, capture.Kind);
        Assert.IsTrue(capture.IsCapture);
        Assert.DoesNotContain(move => move.To == friendlyRookSquare, game.GetLegalMoves(ministerSquare));
    }

    /// <summary>
    /// Verifies an atomic bishop-reachable exchange with a friendly Queen.
    /// </summary>
    [TestMethod]
    public void MinisterAtomicallyExchangesWithFriendlyQueen()
    {
        BoardSquare ministerSquare = new(7, 2);
        BoardSquare queenSquare = new(4, 5);
        ChessGame game = CreateQueenExchangeGame(
            ministerSquare,
            queenSquare);
        ChessMove exchange = FindExchange(
            game,
            ministerSquare,
            queenSquare);

        Assert.IsTrue(exchange.IsExchange);
        Assert.IsFalse(exchange.IsCapture);
        Assert.IsTrue(game.TryMakeMove(
            exchange,
            out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.AreEqual(
            PieceKind.Queen,
            result.ExchangedPiece!.Value.Kind);
        Assert.IsNull(result.CapturedPiece);
        Assert.IsNull(result.CapturedSquare);
        Assert.AreEqual(
            PieceKind.Minister,
            game.GetPiece(queenSquare)!.Value.Kind);
        Assert.AreEqual(
            PieceKind.Queen,
            game.GetPiece(ministerSquare)!.Value.Kind);
        Assert.AreEqual(PieceSide.Black, game.SideToMove);
        Assert.AreEqual(1, game.HalfMoveClock);
        Assert.IsEmpty(game.GetCapturedPieces(PieceSide.White));
    }

    /// <summary>
    /// Verifies an atomic knight-reachable exchange with a friendly King.
    /// </summary>
    [TestMethod]
    public void MinisterAtomicallyExchangesWithFriendlyKing()
    {
        BoardSquare ministerSquare = new(7, 5);
        BoardSquare kingSquare = new(5, 6);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                kingSquare.Rank,
                kingSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                11)
        ]);
        ChessMove exchange = FindExchange(
            game,
            ministerSquare,
            kingSquare);

        Assert.IsTrue(game.TryMakeMove(
            exchange,
            out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.AreEqual(
            PieceKind.King,
            result.ExchangedPiece!.Value.Kind);
        Assert.AreEqual(
            PieceKind.Minister,
            game.GetPiece(kingSquare)!.Value.Kind);
        Assert.AreEqual(
            PieceKind.King,
            game.GetPiece(ministerSquare)!.Value.Kind);
        Assert.IsFalse(game.IsInCheck(PieceSide.White));
    }

    /// <summary>
    /// Verifies that an exchange cannot move the King onto an attacked square.
    /// </summary>
    [TestMethod]
    public void MinisterCannotExchangeKingOntoAttackedSquare()
    {
        BoardSquare ministerSquare = new(7, 5);
        BoardSquare kingSquare = new(5, 6);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                kingSquare.Rank,
                kingSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                11),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                7,
                0)
        ]);

        Assert.DoesNotContain(move => move.To == kingSquare &&
                move.Kind == MoveKind.MinisterExchange, game.GetLegalMoves(ministerSquare));
    }

    /// <summary>
    /// Verifies that a King may escape check through a safe Minister exchange.
    /// </summary>
    [TestMethod]
    public void MinisterKingExchangeMayResolveCheck()
    {
        BoardSquare ministerSquare = new(7, 5);
        BoardSquare kingSquare = new(5, 6);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                kingSquare.Rank,
                kingSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                10),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                5,
                0)
        ]);

        Assert.AreEqual(GameStatus.Check, game.Status);
        ChessMove exchange = FindExchange(
            game,
            ministerSquare,
            kingSquare);

        Assert.IsTrue(game.TryMakeMove(
            exchange,
            out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.IsFalse(game.IsInCheck(PieceSide.White));
        Assert.AreEqual(
            PieceKind.King,
            game.GetPiece(ministerSquare)!.Value.Kind);
    }

    /// <summary>
    /// Verifies that a Queen exchange cannot leave an existing check unresolved.
    /// </summary>
    [TestMethod]
    public void MinisterQueenExchangeCannotLeaveKingInCheck()
    {
        BoardSquare ministerSquare = new(7, 5);
        BoardSquare queenSquare = new(6, 3);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                queenSquare.Rank,
                queenSquare.File),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                5,
                6),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                10),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                5,
                0)
        ]);

        Assert.AreEqual(GameStatus.Check, game.Status);
        Assert.DoesNotContain(move => move.To == queenSquare &&
                move.Kind == MoveKind.MinisterExchange, game.GetLegalMoves(ministerSquare));
    }

    /// <summary>
    /// Verifies that both Minister movement patterns contribute to check.
    /// </summary>
    /// <param name="ministerRank">The Minister's test rank.</param>
    /// <param name="ministerFile">The Minister's test file.</param>
    [TestMethod]
    [DataRow(7, 2)]
    [DataRow(6, 4)]
    public void MinisterGivesCheckByBishopAndKnightReach(
        int ministerRank,
        int ministerFile)
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                11,
                0),
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerRank,
                ministerFile),
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
    /// Verifies that exchanging the King permanently clears its castling rights.
    /// </summary>
    [TestMethod]
    public void MinisterKingExchangeClearsOnlyThatSidesCastlingRights()
    {
        BoardSquare ministerSquare = new(9, 5);
        BoardSquare kingSquare = new(11, 6);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
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
                6),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                0,
                0),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                0,
                11)
        ],
        PieceSide.White,
        CastlingRights.All);
        ChessMove exchange = FindExchange(
            game,
            ministerSquare,
            kingSquare);

        Assert.IsTrue(game.TryMakeMove(
            exchange,
            out MoveResult? result));
        Assert.IsNotNull(result);

        CastlingRights expected =
            CastlingRights.BlackTowardLowerFile |
            CastlingRights.BlackTowardHigherFile;
        Assert.AreEqual(expected, game.CastlingRights);
    }

    /// <summary>
    /// Verifies that exchanging a Queen does not alter castling rights.
    /// </summary>
    [TestMethod]
    public void MinisterQueenExchangePreservesCastlingRights()
    {
        BoardSquare ministerSquare = new(7, 2);
        BoardSquare queenSquare = new(4, 5);
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.Minister,
                PieceSide.White,
                ministerSquare.Rank,
                ministerSquare.File),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                queenSquare.Rank,
                queenSquare.File),
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
                6),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                0,
                0),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                0,
                11)
        ],
        PieceSide.White,
        CastlingRights.All);
        ChessMove exchange = FindExchange(
            game,
            ministerSquare,
            queenSquare);

        Assert.IsTrue(game.TryMakeMove(
            exchange,
            out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.AreEqual(CastlingRights.All, game.CastlingRights);
    }

    /// <summary>
    /// Verifies that cloning and undo preserve Minister exchange state.
    /// </summary>
    [TestMethod]
    public void CloneAndUndoPreserveMinisterExchangeState()
    {
        BoardSquare ministerSquare = new(7, 2);
        BoardSquare queenSquare = new(4, 5);
        ChessGame original = CreateQueenExchangeGame(
            ministerSquare,
            queenSquare);
        ChessGame clone = original.Clone();
        ChessMove exchange = FindExchange(
            clone,
            ministerSquare,
            queenSquare);

        Assert.IsTrue(clone.TryMakeMove(
            exchange,
            out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.AreEqual(
            PieceKind.Minister,
            original.GetPiece(ministerSquare)!.Value.Kind);
        Assert.AreEqual(
            PieceKind.Queen,
            original.GetPiece(queenSquare)!.Value.Kind);

        Assert.IsTrue(clone.UndoLastMove());
        Assert.AreEqual(
            PieceKind.Minister,
            clone.GetPiece(ministerSquare)!.Value.Kind);
        Assert.AreEqual(
            PieceKind.Queen,
            clone.GetPiece(queenSquare)!.Value.Kind);
        Assert.AreEqual(PieceSide.White, clone.SideToMove);
        Assert.IsEmpty(clone.MoveHistory);
        Assert.IsFalse(clone.CanUndo);
    }

    /// <summary>
    /// Verifies that initial horizontal adjacency does not permit an exchange.
    /// </summary>
    [TestMethod]
    public void InitialMinistersCannotExchangeHorizontallyWithRoyalPieces()
    {
        ChessGame game = ChessGame.CreateInitial();
        BoardSquare queenSideMinister = new(11, 4);
        BoardSquare queenSquare = new(11, 5);
        BoardSquare kingSideMinister = new(11, 7);
        BoardSquare kingSquare = new(11, 6);

        Assert.DoesNotContain(move => move.To == queenSquare, game.GetLegalMoves(queenSideMinister));
        Assert.DoesNotContain(move => move.To == kingSquare, game.GetLegalMoves(kingSideMinister));
    }

    /// <summary>
    /// Verifies that regular pawn promotion includes both custom pieces.
    /// </summary>
    [TestMethod]
    public void PawnPromotionOffersMinisterAndChancellor()
    {
        BoardSquare pawnSquare = new(1, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                pawnSquare.Rank,
                pawnSquare.File));

        ChessMove[] promotionMoves = [.. game.GetLegalMoves(pawnSquare).Where(static move => move.IsPromotion)];

        Assert.HasCount(6, promotionMoves);
        Assert.Contains(static move =>
            move.PromotionPiece == PieceKind.Minister, promotionMoves);
        Assert.Contains(static move =>
            move.PromotionPiece == PieceKind.Chancellor, promotionMoves);
    }

    private static ChessGame CreateQueenExchangeGame(
        BoardSquare ministerSquare,
        BoardSquare queenSquare)
    {
        return TestGameFactory.Create(
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
    }

    private static ChessMove FindExchange(
        ChessGame game,
        BoardSquare ministerSquare,
        BoardSquare royalSquare)
    {
        return game.GetLegalMoves(ministerSquare)
            .Single(move =>
                move.To == royalSquare &&
                move.Kind == MoveKind.MinisterExchange);
    }
}
