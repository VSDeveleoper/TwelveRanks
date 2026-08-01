using TwelveRanks.Core;

namespace TwelveRanks.Computer;

/// <summary>
/// Implements an offline opponent using bounded game-tree search.
/// </summary>
public sealed class ConventionalComputerOpponent : IComputerOpponent
{
    private readonly int? _openingSelectionSeed;

    /// <summary>
    /// Initializes an opponent with varied opening play.
    /// </summary>
    public ConventionalComputerOpponent()
    {
    }

    /// <summary>
    /// Initializes an opponent with a reproducible source of opening variety.
    /// </summary>
    /// <param name="randomSeed">
    /// The seed used to select among near-equivalent opening moves.
    /// </param>
    public ConventionalComputerOpponent(int randomSeed)
    {
        _openingSelectionSeed = randomSeed;
    }

    /// <inheritdoc />
    public ComputerMoveDecision ChooseMove(
        ChessGame game,
        ComputerSearchLimits limits,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(limits);
        cancellationToken.ThrowIfCancellationRequested();

        ConventionalSearch search = new(
            game,
            limits,
            _openingSelectionSeed ??
                System.Security.Cryptography.RandomNumberGenerator.GetInt32(
                    int.MaxValue),
            cancellationToken);

        return search.Run();
    }
}
