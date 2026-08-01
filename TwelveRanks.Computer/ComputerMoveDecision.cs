using TwelveRanks.Core;

namespace TwelveRanks.Computer;

/// <summary>
/// Reports an immutable move choice and its search diagnostics.
/// </summary>
public sealed record ComputerMoveDecision
{
    internal ComputerMoveDecision(
        ChessMove? move,
        int score,
        int completedDepth,
        long nodesSearched,
        TimeSpan elapsed,
        bool reachedTimeLimit)
    {
        Move = move;
        Score = score;
        CompletedDepth = completedDepth;
        NodesSearched = nodesSearched;
        Elapsed = elapsed;
        ReachedTimeLimit = reachedTimeLimit;
    }

    /// <summary>
    /// Gets the selected legal move, or no value for a terminal position.
    /// </summary>
    public ChessMove? Move { get; }

    /// <summary>
    /// Gets a value indicating whether the decision contains a move.
    /// </summary>
    public bool HasMove => Move.HasValue;

    /// <summary>
    /// Gets the score in centipawn-like units from the searching side's
    /// perspective.
    /// </summary>
    /// <remarks>
    /// Mate scores are deliberately much larger than heuristic scores. The
    /// exact heuristic scale is an implementation detail and may evolve.
    /// </remarks>
    public int Score { get; }

    /// <summary>
    /// Gets the deepest fully completed principal search depth in plies.
    /// </summary>
    public int CompletedDepth { get; }

    /// <summary>
    /// Gets the number of principal and tactical nodes visited.
    /// </summary>
    public long NodesSearched { get; }

    /// <summary>
    /// Gets the total elapsed search time.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets a value indicating whether the time limit ended the search.
    /// </summary>
    public bool ReachedTimeLimit { get; }
}
