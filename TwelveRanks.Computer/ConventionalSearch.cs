using TwelveRanks.Core;

namespace TwelveRanks.Computer;

internal sealed class ConventionalSearch(
    ChessGame sourceGame,
    ComputerSearchLimits limits,
    int openingSelectionSeed,
    CancellationToken cancellationToken)
{
    private const int MinimumOpeningPieceCount = 40;
    private const int OpeningFullMoveLimit = 4;
    private const int OpeningVarietyScoreWindow = 12;

    private readonly SearchClock _clock = new(
            limits.TimeLimit,
            cancellationToken);
    private readonly ChessGame _game = sourceGame.Clone();
    private readonly ComputerSearchLimits _limits = limits;
    private readonly int _openingSelectionSeed = openingSelectionSeed;

    public ComputerMoveDecision Run()
    {
        _clock.ThrowIfCancellationRequested();

        IReadOnlyList<ChessMove> legalMoves = _game.GetLegalMoves();

        if (legalMoves.Count == 0)
        {
            return CreateTerminalDecision();
        }

        List<ChessMove> orderedMoves =
            MoveOrderer.Order(_game, legalMoves);
        ChessMove selectedMove = orderedMoves[0];
        int selectedScore = 0;
        int completedDepth = 0;
        bool deadlineInterruptedSearch = false;

        for (int depth = 1; depth <= _limits.MaximumDepth; depth++)
        {
            try
            {
                SearchIteration iteration =
                    SearchRoot(depth, selectedMove);

                selectedMove = iteration.Move;
                selectedScore = iteration.Score;
                completedDepth = depth;
            }
            catch (TimeoutException)
            {
                deadlineInterruptedSearch = true;
                break;
            }
        }

        return new ComputerMoveDecision(
            selectedMove,
            selectedScore,
            completedDepth,
            _clock.NodesSearched,
            _clock.Elapsed,
            deadlineInterruptedSearch || _clock.HasReachedTimeLimit);
    }

    private ComputerMoveDecision CreateTerminalDecision()
    {
        int score = _game.IsGameOver
            ? PositionEvaluator.GetTerminalScore(_game.Status, 0)
            : 0;

        return new ComputerMoveDecision(
            move: null,
            score,
            completedDepth: 0,
            _clock.NodesSearched,
            _clock.Elapsed,
            reachedTimeLimit: false);
    }

    private SearchIteration SearchRoot(
        int depth,
        ChessMove preferredMove)
    {
        _clock.ThrowIfSearchMustStop();

        List<ChessMove> moves = MoveOrderer.Order(
            _game,
            _game.GetLegalMoves(),
            preferredMove);
        bool useOpeningVariety = ShouldUseOpeningVariety();
        List<ScoredMove>? scoredMoves = useOpeningVariety
            ? []
            : null;
        ChessMove bestMove = moves[0];
        int bestScore = -PositionEvaluator.Infinity;
        int alpha = -PositionEvaluator.Infinity;

        foreach (ChessMove move in moves)
        {
            int score = SearchPrincipalMove(
                move,
                depth - 1,
                -PositionEvaluator.Infinity,
                useOpeningVariety
                    ? PositionEvaluator.Infinity
                    : -alpha,
                ply: 1);

            scoredMoves?.Add(new ScoredMove(move, score));

            if (score > bestScore)
            {
                bestMove = move;
                bestScore = score;
            }

            alpha = Math.Max(alpha, score);
        }

        if (scoredMoves is not null)
        {
            return SelectOpeningMove(scoredMoves, bestScore);
        }

        return new SearchIteration(bestMove, bestScore);
    }

    private bool ShouldUseOpeningVariety()
    {
        if (_game.FullMoveNumber > OpeningFullMoveLimit)
        {
            return false;
        }

        int pieceCount = 0;

        foreach (PiecePlacement _ in _game.GetPieces())
        {
            pieceCount++;
        }

        return pieceCount >= MinimumOpeningPieceCount;
    }

    private SearchIteration SelectOpeningMove(
        IReadOnlyList<ScoredMove> scoredMoves,
        int bestScore)
    {
        List<ScoredMove> candidates = [];

        foreach (ScoredMove scoredMove in scoredMoves)
        {
            if (scoredMove.Score >=
                bestScore - OpeningVarietyScoreWindow)
            {
                candidates.Add(scoredMove);
            }
        }

        int selectionIndex = (int)(
            (uint)_openingSelectionSeed %
            (uint)candidates.Count);
        ScoredMove selectedMove = candidates[selectionIndex];

        return new SearchIteration(
            selectedMove.Move,
            selectedMove.Score);
    }

    private int Negamax(
        int depth,
        int alpha,
        int beta,
        int ply)
    {
        if (depth <= 0)
        {
            return Quiescence(
                alpha,
                beta,
                _limits.MaximumQuiescenceDepth,
                ply);
        }

        _clock.VisitNode();

        if (_game.IsGameOver)
        {
            return PositionEvaluator.GetTerminalScore(
                _game.Status,
                ply);
        }

        List<ChessMove> moves = MoveOrderer.Order(
            _game,
            _game.GetLegalMoves());

        return SearchPrincipalMoves(
            moves,
            depth,
            alpha,
            beta,
            ply);
    }

    private int SearchPrincipalMoves(
        IReadOnlyList<ChessMove> moves,
        int depth,
        int alpha,
        int beta,
        int ply)
    {
        int bestScore = -PositionEvaluator.Infinity;

        foreach (ChessMove move in moves)
        {
            int score = SearchPrincipalMove(
                move,
                depth - 1,
                -beta,
                -alpha,
                ply + 1);
            bestScore = Math.Max(bestScore, score);
            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                break;
            }
        }

        return bestScore;
    }

    private int SearchPrincipalMove(
        ChessMove move,
        int depth,
        int alpha,
        int beta,
        int ply)
    {
        ApplyMove(move);

        try
        {
            return -Negamax(depth, alpha, beta, ply);
        }
        finally
        {
            UndoMove();
        }
    }

    private int Quiescence(
        int alpha,
        int beta,
        int remainingDepth,
        int ply)
    {
        _clock.VisitNode();

        if (_game.IsGameOver)
        {
            return PositionEvaluator.GetTerminalScore(
                _game.Status,
                ply);
        }

        int standPat = PositionEvaluator.Evaluate(_game);

        if (remainingDepth == 0)
        {
            return standPat;
        }

        bool mustEvadeCheck = _game.Status == GameStatus.Check;

        if (!mustEvadeCheck)
        {
            if (standPat >= beta)
            {
                return standPat;
            }

            alpha = Math.Max(alpha, standPat);
        }

        List<ChessMove> tacticalMoves =
            GetQuiescenceMoves(mustEvadeCheck);

        return SearchQuiescenceMoves(
            tacticalMoves,
            alpha,
            beta,
            remainingDepth,
            ply);
    }

    private List<ChessMove> GetQuiescenceMoves(bool mustEvadeCheck)
    {
        IReadOnlyList<ChessMove> legalMoves = _game.GetLegalMoves();

        if (mustEvadeCheck)
        {
            return MoveOrderer.Order(_game, legalMoves);
        }

        List<ChessMove> tacticalMoves = [];

        foreach (ChessMove move in legalMoves)
        {
            if (MoveOrderer.IsTactical(move))
            {
                tacticalMoves.Add(move);
            }
        }

        return MoveOrderer.Order(_game, tacticalMoves);
    }

    private int SearchQuiescenceMoves(
        IReadOnlyList<ChessMove> moves,
        int alpha,
        int beta,
        int remainingDepth,
        int ply)
    {
        foreach (ChessMove move in moves)
        {
            int score = SearchQuiescenceMove(
                move,
                -beta,
                -alpha,
                remainingDepth - 1,
                ply + 1);
            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                break;
            }
        }

        return alpha;
    }

    private int SearchQuiescenceMove(
        ChessMove move,
        int alpha,
        int beta,
        int remainingDepth,
        int ply)
    {
        ApplyMove(move);

        try
        {
            return -Quiescence(
                alpha,
                beta,
                remainingDepth,
                ply);
        }
        finally
        {
            UndoMove();
        }
    }

    private void ApplyMove(ChessMove move)
    {
        if (!_game.TryMakeMove(move, out _))
        {
            throw new InvalidOperationException(
                "The engine rejected a move that it generated as legal.");
        }
    }

    private void UndoMove()
    {
        if (!_game.UndoLastMove())
        {
            throw new InvalidOperationException(
                "The analysis position could not undo its last move.");
        }
    }

    private readonly record struct SearchIteration(
        ChessMove Move,
        int Score);

    private readonly record struct ScoredMove(
        ChessMove Move,
        int Score);
}
