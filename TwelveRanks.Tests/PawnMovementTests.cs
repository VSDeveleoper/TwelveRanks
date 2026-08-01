using TwelveRanks.Core;

namespace TwelveRanks.Tests;

/// <summary>
/// Verifies regular-pawn and Royal Guard pawn movement.
/// </summary>
[TestClass]
public sealed class PawnMovementTests
{
    /// <summary>
    /// Verifies the regular pawn's initial one- and two-square advances.
    /// </summary>
    [TestMethod]
    public void RegularWhitePawnMayAdvanceOneOrTwoSquaresInitially()
    {
        BoardSquare from = new(10, 4);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, from);

        Assert.AreSequenceEqual(
            [
                new BoardSquare(9, 4),
                new BoardSquare(8, 4)
            ], [.. destinations], Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);
    }

    /// <summary>
    /// Verifies that the regular pawn loses its double advance after moving.
    /// </summary>
    [TestMethod]
    public void RegularPawnCannotAdvanceTwoSquaresAfterItsFirstMove()
    {
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                10,
                4));

        Apply(game, new BoardSquare(10, 4), new BoardSquare(8, 4));
        Apply(game, new BoardSquare(0, 11), new BoardSquare(1, 11));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, new BoardSquare(8, 4));

        Assert.Contains(new BoardSquare(7, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(6, 4), destinations);
    }

    /// <summary>
    /// Verifies that a blocker prevents both straight pawn advances.
    /// </summary>
    [TestMethod]
    public void OccupiedForwardSquareBlocksRegularPawn()
    {
        BoardSquare from = new(10, 4);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                9,
                4));

        Assert.IsEmpty(game.GetLegalMoves(from));
    }

    /// <summary>
    /// Verifies that regular pawns capture only diagonally forward.
    /// </summary>
    [TestMethod]
    public void RegularPawnCapturesForwardDiagonalButNeverBackwardOrSideways()
    {
        BoardSquare from = new(7, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                6,
                4),
            TestGameFactory.Place(
                PieceKind.Bishop,
                PieceSide.Black,
                8,
                4));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, from);

        Assert.Contains(new BoardSquare(6, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(6, 6), destinations);
        Assert.DoesNotContain(new BoardSquare(8, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(8, 6), destinations);
        Assert.DoesNotContain(new BoardSquare(8, 5), destinations);
        Assert.DoesNotContain(new BoardSquare(7, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(7, 6), destinations);
    }

    /// <summary>
    /// Verifies every added movement of a White Royal Guard pawn.
    /// </summary>
    [TestMethod]
    public void RoyalGuardPawnMovesForwardBackwardAndCapturesBothWays()
    {
        BoardSquare from = new(7, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File,
                PawnRole.QueenPawn),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                6,
                4),
            TestGameFactory.Place(
                PieceKind.Bishop,
                PieceSide.Black,
                6,
                6),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                8,
                4),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.Black,
                8,
                6));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, from);

        BoardSquare[] expected =
        [
            new BoardSquare(6, 5),
            new BoardSquare(5, 5),
            new BoardSquare(8, 5),
            new BoardSquare(6, 4),
            new BoardSquare(6, 6),
            new BoardSquare(8, 4),
            new BoardSquare(8, 6)
        ];

        Assert.AreSequenceEqual(expected, [.. destinations], Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);
    }

    /// <summary>
    /// Verifies one-square quiet diagonal movement for both Royal Guard sides.
    /// </summary>
    /// <param name="side">The Royal Guard's side.</param>
    /// <param name="role">The Royal Guard's origin role.</param>
    /// <param name="fromRank">The Royal Guard's starting rank.</param>
    /// <param name="forwardRank">The rank one square forward.</param>
    /// <param name="backwardRank">The rank one square backward.</param>
    [TestMethod]
    [DataRow(PieceSide.White, PawnRole.KingPawn, 7, 6, 8)]
    [DataRow(PieceSide.Black, PawnRole.QueenPawn, 4, 5, 3)]
    public void RoyalGuardPawnMayMoveOneSquareDiagonallyToEmptySquares(
        PieceSide side,
        PawnRole role,
        int fromRank,
        int forwardRank,
        int backwardRank)
    {
        BoardSquare from = new(fromRank, 5);
        ChessGame game = TestGameFactory.Create(
            side,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                side,
                from.Rank,
                from.File,
                role));

        IReadOnlyList<ChessMove> moves = game.GetLegalMoves(from);
        BoardSquare[] diagonalTargets =
        [
            new BoardSquare(forwardRank, 4),
            new BoardSquare(forwardRank, 6),
            new BoardSquare(backwardRank, 4),
            new BoardSquare(backwardRank, 6)
        ];

        foreach (BoardSquare target in diagonalTargets)
        {
            ChessMove move = moves.Single(candidate => candidate.To == target);
            Assert.AreEqual(MoveKind.Quiet, move.Kind);
        }

        HashSet<BoardSquare> destinations =
            [.. moves.Select(static move => move.To)];
        Assert.DoesNotContain(
            new BoardSquare(from.Rank, from.File - 1),
            destinations);
        Assert.DoesNotContain(
            new BoardSquare(from.Rank, from.File + 1),
            destinations);
    }

    /// <summary>
    /// Verifies that friendly pieces block all four Royal Guard diagonals.
    /// </summary>
    /// <param name="side">The Royal Guard's side.</param>
    /// <param name="role">The Royal Guard's origin role.</param>
    /// <param name="fromRank">The Royal Guard's starting rank.</param>
    /// <param name="forwardRank">The rank one square forward.</param>
    /// <param name="backwardRank">The rank one square backward.</param>
    [TestMethod]
    [DataRow(PieceSide.White, PawnRole.QueenPawn, 7, 6, 8)]
    [DataRow(PieceSide.Black, PawnRole.KingPawn, 4, 5, 3)]
    public void RoyalGuardPawnCannotMoveOntoFriendlyDiagonalPieces(
        PieceSide side,
        PawnRole role,
        int fromRank,
        int forwardRank,
        int backwardRank)
    {
        BoardSquare from = new(fromRank, 5);
        ChessGame game = TestGameFactory.Create(
            side,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                side,
                from.Rank,
                from.File,
                role),
            TestGameFactory.Place(
                PieceKind.Knight,
                side,
                forwardRank,
                4),
            TestGameFactory.Place(
                PieceKind.Knight,
                side,
                forwardRank,
                6),
            TestGameFactory.Place(
                PieceKind.Knight,
                side,
                backwardRank,
                4),
            TestGameFactory.Place(
                PieceKind.Knight,
                side,
                backwardRank,
                6));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, from);

        Assert.DoesNotContain(new BoardSquare(forwardRank, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(forwardRank, 6), destinations);
        Assert.DoesNotContain(new BoardSquare(backwardRank, 4), destinations);
        Assert.DoesNotContain(new BoardSquare(backwardRank, 6), destinations);
    }

    /// <summary>
    /// Verifies that a Royal Guard retains its double advance after moving.
    /// </summary>
    [TestMethod]
    public void RoyalGuardPawnMayAdvanceTwoSquaresAtAnyTime()
    {
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                10,
                5,
                PawnRole.QueenPawn));

        Apply(game, new BoardSquare(10, 5), new BoardSquare(8, 5));
        Apply(game, new BoardSquare(0, 11), new BoardSquare(1, 11));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, new BoardSquare(8, 5));

        Assert.Contains(new BoardSquare(6, 5), destinations);
        Assert.Contains(new BoardSquare(9, 5), destinations);
    }

    /// <summary>
    /// Verifies the same forward and backward rules for a Black Royal Guard.
    /// </summary>
    [TestMethod]
    public void BlackRoyalGuardPawnUsesTheMirroredDirection()
    {
        BoardSquare from = new(4, 5);
        ChessGame game = TestGameFactory.Create(
            PieceSide.Black,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.Black,
                from.Rank,
                from.File,
                PawnRole.KingPawn),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.White,
                5,
                4),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.White,
                5,
                6),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.White,
                3,
                4),
            TestGameFactory.Place(
                PieceKind.Queen,
                PieceSide.White,
                3,
                6));

        IReadOnlyList<ChessMove> moves = game.GetLegalMoves(from);
        HashSet<BoardSquare> destinations = [.. moves.Select(
            static move => move.To)];

        Assert.Contains(new BoardSquare(5, 5), destinations);
        Assert.Contains(new BoardSquare(6, 5), destinations);
        Assert.Contains(new BoardSquare(3, 5), destinations);
        Assert.DoesNotContain(new BoardSquare(2, 5), destinations);
        Assert.AreEqual(
            MoveKind.Capture,
            moves.Single(move => move.To == new BoardSquare(5, 4)).Kind);
        Assert.AreEqual(
            MoveKind.Capture,
            moves.Single(move => move.To == new BoardSquare(5, 6)).Kind);
        Assert.AreEqual(
            MoveKind.Capture,
            moves.Single(move => move.To == new BoardSquare(3, 4)).Kind);
        Assert.AreEqual(
            MoveKind.Capture,
            moves.Single(move => move.To == new BoardSquare(3, 6)).Kind);
    }

    /// <summary>
    /// Verifies that Royal Guard backward diagonals contribute to check.
    /// </summary>
    [TestMethod]
    public void RoyalGuardPawnAttacksBackwardDiagonally()
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                11,
                11),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                2,
                2),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                1,
                1,
                PawnRole.QueenPawn)
        ],
        PieceSide.Black);

        Assert.IsTrue(game.IsInCheck(PieceSide.Black));
        Assert.AreEqual(GameStatus.Check, game.Status);
    }

    /// <summary>
    /// Verifies the mirrored Black Royal Guard backward attack map.
    /// </summary>
    [TestMethod]
    public void BlackRoyalGuardPawnAttacksBackwardDiagonally()
    {
        ChessGame game = ChessGame.CreateCustom(
        [
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.White,
                9,
                2),
            TestGameFactory.Place(
                PieceKind.King,
                PieceSide.Black,
                0,
                11),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.Black,
                10,
                1,
                PawnRole.KingPawn)
        ],
        PieceSide.White);

        Assert.IsTrue(game.IsInCheck(PieceSide.White));
        Assert.AreEqual(GameStatus.Check, game.Status);
    }

    /// <summary>
    /// Verifies en passant after a Royal Guard advances two squares.
    /// </summary>
    [TestMethod]
    public void RoyalGuardPawnDoubleAdvanceCanBeCapturedEnPassant()
    {
        ChessGame game = TestGameFactory.Create(
            PieceSide.Black,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.Black,
                4,
                5,
                PawnRole.QueenPawn),
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                6,
                4,
                PawnRole.KingPawn));

        Apply(game, new BoardSquare(4, 5), new BoardSquare(6, 5));

        BoardSquare whitePawn = new(6, 4);
        BoardSquare target = new(5, 5);
        ChessMove[] matchingMoves =
        [
            .. game.GetLegalMoves(whitePawn).Where(move => move.To == target)
        ];

        Assert.HasCount(1, matchingMoves);
        ChessMove enPassant = matchingMoves[0];
        Assert.AreEqual(MoveKind.EnPassant, enPassant.Kind);
        Assert.IsTrue(game.TryMakeMove(enPassant, out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.IsNull(game.GetPiece(new BoardSquare(6, 5)));
        BoardPiece royalGuard = game.GetPiece(target)!.Value;
        Assert.AreEqual(PieceSide.White, royalGuard.Side);
        Assert.AreEqual(PawnRole.KingPawn, royalGuard.PawnRole);
    }

    /// <summary>
    /// Verifies all supported promotion choices and the resulting identity.
    /// </summary>
    [TestMethod]
    public void RegularPawnPromotionOffersSixSupportedPieces()
    {
        BoardSquare from = new(1, 5);
        BoardSquare target = new(0, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File));

        ChessMove[] promotions = [.. game.GetLegalMoves(from).Where(move => move.To == target)];

        Assert.HasCount(6, promotions);
        Assert.AreSequenceEqual(
            [
                PieceKind.Queen,
                PieceKind.Rook,
                PieceKind.Bishop,
                PieceKind.Knight,
                PieceKind.Minister,
                PieceKind.Chancellor
            ], [.. promotions
                .Select(static move =>
                    move.PromotionPiece.GetValueOrDefault())], Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);

        ChessMove queenPromotion = promotions.Single(
            static move => move.PromotionPiece == PieceKind.Queen);

        Assert.IsTrue(game.TryMakeMove(
            queenPromotion,
            out MoveResult? result));
        Assert.IsNotNull(result);

        BoardPiece promotedPiece = game.GetPiece(target)!.Value;
        Assert.AreEqual(PieceKind.Queen, promotedPiece.Kind);
        Assert.AreEqual(PawnRole.Regular, promotedPiece.PawnRole);
    }

    /// <summary>
    /// Verifies that a regular pawn still receives all six promotion choices
    /// when capturing on the final rank.
    /// </summary>
    [TestMethod]
    public void RegularPawnPromotionCaptureOffersSixSupportedPieces()
    {
        BoardSquare from = new(1, 5);
        BoardSquare target = new(0, 4);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File),
            TestGameFactory.Place(
                PieceKind.Rook,
                PieceSide.Black,
                target.Rank,
                target.File));

        ChessMove[] promotions =
        [
            .. game.GetLegalMoves(from).Where(move => move.To == target)
        ];

        Assert.HasCount(6, promotions);
        Assert.IsTrue(promotions.All(
            static move =>
                move.Kind == MoveKind.PromotionCapture &&
                move.PromotionPiece.HasValue));
        Assert.AreSequenceEqual(
            [
                PieceKind.Queen,
                PieceKind.Rook,
                PieceKind.Bishop,
                PieceKind.Knight,
                PieceKind.Minister,
                PieceKind.Chancellor
            ], [.. promotions
                .Select(static move =>
                    move.PromotionPiece.GetValueOrDefault())], Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);
    }

    /// <summary>
    /// Verifies that a Royal Guard cannot jump over an occupied square.
    /// </summary>
    [TestMethod]
    public void RoyalGuardPawnCannotJumpDuringItsDoubleAdvance()
    {
        BoardSquare from = new(7, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File,
                PawnRole.KingPawn),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                6,
                5));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, from);

        Assert.DoesNotContain(new BoardSquare(6, 5), destinations);
        Assert.DoesNotContain(new BoardSquare(5, 5), destinations);
    }

    /// <summary>
    /// Verifies that a Royal Guard cannot move or capture two squares diagonally.
    /// </summary>
    [TestMethod]
    public void RoyalGuardPawnCannotMoveTwoSquaresDiagonally()
    {
        BoardSquare from = new(7, 5);
        ChessGame game = TestGameFactory.Create(
            TestGameFactory.Place(
                PieceKind.Pawn,
                PieceSide.White,
                from.Rank,
                from.File,
                PawnRole.QueenPawn),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                5,
                3),
            TestGameFactory.Place(
                PieceKind.Knight,
                PieceSide.Black,
                9,
                7));

        HashSet<BoardSquare> destinations =
            TestGameFactory.Destinations(game, from);

        Assert.DoesNotContain(new BoardSquare(5, 3), destinations);
        Assert.DoesNotContain(new BoardSquare(5, 7), destinations);
        Assert.DoesNotContain(new BoardSquare(9, 3), destinations);
        Assert.DoesNotContain(new BoardSquare(9, 7), destinations);
    }

    /// <summary>
    /// Verifies quiet one- and two-square final-rank arrivals for both sides
    /// and both Royal Guard origin roles.
    /// </summary>
    /// <param name="side">The Royal Guard's side.</param>
    /// <param name="role">The Royal Guard's origin role.</param>
    /// <param name="fromRank">The Royal Guard's starting rank.</param>
    /// <param name="targetRank">The opponent's final rank.</param>
    [TestMethod]
    [DataRow(PieceSide.White, PawnRole.QueenPawn, 1, 0)]
    [DataRow(PieceSide.White, PawnRole.KingPawn, 2, 0)]
    [DataRow(PieceSide.Black, PawnRole.QueenPawn, 10, 11)]
    [DataRow(PieceSide.Black, PawnRole.KingPawn, 9, 11)]
    public void RoyalGuardPawnDoesNotPromoteOnAQuietFinalRankArrival(
        PieceSide side,
        PawnRole role,
        int fromRank,
        int targetRank)
    {
        BoardSquare from = new(fromRank, 5);
        BoardSquare target = new(targetRank, 5);
        ChessGame game = TestGameFactory.Create(
            side,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                side,
                from.Rank,
                from.File,
                role));

        ChessMove[] matchingMoves =
        [
            .. game.GetLegalMoves(from).Where(move => move.To == target)
        ];

        Assert.HasCount(1, matchingMoves);
        ChessMove move = matchingMoves[0];
        Assert.AreEqual(MoveKind.Quiet, move.Kind);
        Assert.IsNull(move.PromotionPiece);
        Assert.IsFalse(move.IsPromotion);
        Assert.IsTrue(game.TryMakeMove(move, out MoveResult? result));
        Assert.IsNotNull(result);

        BoardPiece royalGuard = game.GetPiece(target)!.Value;
        Assert.AreEqual(PieceKind.Pawn, royalGuard.Kind);
        Assert.AreEqual(role, royalGuard.PawnRole);

        if (Math.Abs(targetRank - fromRank) == 2)
        {
            Assert.AreEqual(
                new BoardSquare((fromRank + targetRank) / 2, from.File),
                game.EnPassantTarget);
        }
        else
        {
            Assert.IsNull(game.EnPassantTarget);
        }
    }

    /// <summary>
    /// Verifies that an empty diagonal final-rank arrival remains a quiet
    /// Royal Guard move for both sides.
    /// </summary>
    /// <param name="side">The Royal Guard's side.</param>
    /// <param name="role">The Royal Guard's origin role.</param>
    /// <param name="fromRank">The Royal Guard's starting rank.</param>
    /// <param name="targetRank">The opponent's final rank.</param>
    [TestMethod]
    [DataRow(PieceSide.White, PawnRole.QueenPawn, 1, 0)]
    [DataRow(PieceSide.Black, PawnRole.KingPawn, 10, 11)]
    public void RoyalGuardPawnDoesNotPromoteOnAnEmptyDiagonalFinalRankArrival(
        PieceSide side,
        PawnRole role,
        int fromRank,
        int targetRank)
    {
        BoardSquare from = new(fromRank, 5);
        BoardSquare target = new(targetRank, 4);
        ChessGame game = TestGameFactory.Create(
            side,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                side,
                from.Rank,
                from.File,
                role));

        ChessMove[] matchingMoves =
        [
            .. game.GetLegalMoves(from).Where(move => move.To == target)
        ];

        Assert.HasCount(1, matchingMoves);
        ChessMove move = matchingMoves[0];
        Assert.AreEqual(MoveKind.Quiet, move.Kind);
        Assert.IsNull(move.PromotionPiece);
        Assert.IsFalse(move.IsPromotion);
        Assert.IsTrue(game.TryMakeMove(move, out MoveResult? result));
        Assert.IsNotNull(result);

        BoardPiece royalGuard = game.GetPiece(target)!.Value;
        Assert.AreEqual(PieceKind.Pawn, royalGuard.Kind);
        Assert.AreEqual(role, royalGuard.PawnRole);
    }

    /// <summary>
    /// Verifies final-rank captures for both sides without promotion.
    /// </summary>
    /// <param name="side">The Royal Guard's side.</param>
    /// <param name="role">The Royal Guard's origin role.</param>
    /// <param name="fromRank">The Royal Guard's starting rank.</param>
    /// <param name="targetRank">The opponent's final rank.</param>
    [TestMethod]
    [DataRow(PieceSide.White, PawnRole.QueenPawn, 1, 0)]
    [DataRow(PieceSide.Black, PawnRole.KingPawn, 10, 11)]
    public void RoyalGuardPawnDoesNotPromoteWhenCapturingOnTheFinalRank(
        PieceSide side,
        PawnRole role,
        int fromRank,
        int targetRank)
    {
        BoardSquare from = new(fromRank, 5);
        BoardSquare target = new(targetRank, 4);
        PieceSide opponent = side == PieceSide.White
            ? PieceSide.Black
            : PieceSide.White;
        ChessGame game = TestGameFactory.Create(
            side,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                side,
                from.Rank,
                from.File,
                role),
            TestGameFactory.Place(
                PieceKind.Knight,
                opponent,
                target.Rank,
                target.File));

        ChessMove[] matchingMoves =
        [
            .. game.GetLegalMoves(from).Where(move => move.To == target)
        ];

        Assert.HasCount(1, matchingMoves);
        ChessMove move = matchingMoves[0];
        Assert.AreEqual(MoveKind.Capture, move.Kind);
        Assert.IsNull(move.PromotionPiece);
        Assert.IsFalse(move.IsPromotion);
        Assert.IsTrue(game.TryMakeMove(move, out MoveResult? result));
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.CapturedPiece);

        BoardPiece royalGuard = game.GetPiece(target)!.Value;
        Assert.AreEqual(PieceKind.Pawn, royalGuard.Kind);
        Assert.AreEqual(role, royalGuard.PawnRole);
    }

    /// <summary>
    /// Verifies that a Royal Guard on the final rank remains active and can
    /// move or capture one square backward.
    /// </summary>
    /// <param name="side">The Royal Guard's side.</param>
    /// <param name="role">The Royal Guard's origin role.</param>
    /// <param name="finalRank">The opponent's final rank.</param>
    /// <param name="backwardRank">The rank one square backward.</param>
    [TestMethod]
    [DataRow(PieceSide.White, PawnRole.QueenPawn, 0, 1)]
    [DataRow(PieceSide.Black, PawnRole.KingPawn, 11, 10)]
    public void RoyalGuardPawnCanRetreatFromTheFinalRank(
        PieceSide side,
        PawnRole role,
        int finalRank,
        int backwardRank)
    {
        BoardSquare from = new(finalRank, 5);
        PieceSide opponent = side == PieceSide.White
            ? PieceSide.Black
            : PieceSide.White;
        ChessGame game = TestGameFactory.Create(
            side,
            CastlingRights.None,
            TestGameFactory.Place(
                PieceKind.Pawn,
                side,
                from.Rank,
                from.File,
                role),
            TestGameFactory.Place(
                PieceKind.Knight,
                opponent,
                backwardRank,
                4));

        IReadOnlyList<ChessMove> moves = game.GetLegalMoves(from);

        ChessMove retreat = moves.Single(
            move => move.To == new BoardSquare(backwardRank, 5));
        ChessMove backwardCapture = moves.Single(
            move => move.To == new BoardSquare(backwardRank, 4));
        ChessMove backwardDiagonalMove = moves.Single(
            move => move.To == new BoardSquare(backwardRank, 6));

        Assert.AreEqual(MoveKind.Quiet, retreat.Kind);
        Assert.AreEqual(MoveKind.Capture, backwardCapture.Kind);
        Assert.AreEqual(MoveKind.Quiet, backwardDiagonalMove.Kind);
        Assert.IsFalse(retreat.IsPromotion);
        Assert.IsFalse(backwardCapture.IsPromotion);
        Assert.IsFalse(backwardDiagonalMove.IsPromotion);
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
