using System.Diagnostics;

namespace TwelveRanks.Computer;

internal sealed class SearchClock(
    TimeSpan timeLimit,
    CancellationToken cancellationToken)
{
    private readonly CancellationToken _cancellationToken = cancellationToken;
    private readonly long _startedAt = Stopwatch.GetTimestamp();
    private readonly TimeSpan _timeLimit = timeLimit;

    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(_startedAt);

    public long NodesSearched { get; private set; }

    public bool HasReachedTimeLimit => Elapsed >= _timeLimit;

    public void ThrowIfCancellationRequested()
    {
        _cancellationToken.ThrowIfCancellationRequested();
    }

    public void VisitNode()
    {
        ThrowIfSearchMustStop();
        NodesSearched++;
    }

    public void ThrowIfSearchMustStop()
    {
        ThrowIfCancellationRequested();

        if (HasReachedTimeLimit)
        {
            throw new TimeoutException(
                "The conventional-opponent search reached its time limit.");
        }
    }
}
