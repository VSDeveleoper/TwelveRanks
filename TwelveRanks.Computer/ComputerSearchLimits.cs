namespace TwelveRanks.Computer;

/// <summary>
/// Defines immutable resource limits for one conventional-opponent search.
/// </summary>
public sealed record ComputerSearchLimits
{
    /// <summary>
    /// Initializes search limits.
    /// </summary>
    /// <param name="timeLimit">The maximum intended thinking time.</param>
    /// <param name="maximumDepth">
    /// The greatest fully searched principal depth, measured in plies.
    /// </param>
    /// <param name="maximumQuiescenceDepth">
    /// The greatest additional tactical depth, measured in plies.
    /// </param>
    public ComputerSearchLimits(
        TimeSpan timeLimit,
        int maximumDepth,
        int maximumQuiescenceDepth)
    {
        ValidateTimeLimit(timeLimit);
        ValidateMaximumDepth(maximumDepth);
        ValidateMaximumQuiescenceDepth(maximumQuiescenceDepth);

        TimeLimit = timeLimit;
        MaximumDepth = maximumDepth;
        MaximumQuiescenceDepth = maximumQuiescenceDepth;
    }

    /// <summary>
    /// Gets the initial balanced limits for an ordinary computer game.
    /// </summary>
    public static ComputerSearchLimits Standard { get; } = new(
        TimeSpan.FromSeconds(1),
        maximumDepth: 6,
        maximumQuiescenceDepth: 2);

    /// <summary>
    /// Gets the maximum intended thinking time.
    /// </summary>
    public TimeSpan TimeLimit { get; }

    /// <summary>
    /// Gets the greatest fully searched principal depth in plies.
    /// </summary>
    public int MaximumDepth { get; }

    /// <summary>
    /// Gets the greatest additional tactical depth in plies.
    /// </summary>
    public int MaximumQuiescenceDepth { get; }

    private static void ValidateTimeLimit(TimeSpan timeLimit)
    {
        if (timeLimit <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeLimit),
                timeLimit,
                "The computer thinking time must be positive.");
        }
    }

    private static void ValidateMaximumDepth(int maximumDepth)
    {
        if (maximumDepth < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDepth),
                maximumDepth,
                "The maximum principal depth must be at least one ply.");
        }
    }

    private static void ValidateMaximumQuiescenceDepth(
        int maximumQuiescenceDepth)
    {
        if (maximumQuiescenceDepth < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumQuiescenceDepth),
                maximumQuiescenceDepth,
                "The maximum tactical depth cannot be negative.");
        }
    }
}
