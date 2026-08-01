using TwelveRanks.Core;

namespace TwelveRanks.Computer;

/// <summary>
/// Selects a legal move for a conventional, CPU-bound computer opponent.
/// </summary>
public interface IComputerOpponent
{
    /// <summary>
    /// Searches one game position for the move the computer should play.
    /// </summary>
    /// <param name="game">
    /// The authoritative position to analyze. The implementation must not
    /// modify this instance.
    /// </param>
    /// <param name="limits">The time and depth limits for this search.</param>
    /// <param name="cancellationToken">
    /// A token used to abandon the search. Cancellation is reported with an
    /// <see cref="OperationCanceledException"/>.
    /// </param>
    /// <returns>
    /// The selected move and search diagnostics, or a decision without a move
    /// when the supplied game is already over.
    /// </returns>
    /// <remarks>
    /// This is a synchronous, CPU-bound operation. UI callers should invoke it
    /// on a worker thread.
    /// </remarks>
    ComputerMoveDecision ChooseMove(
        ChessGame game,
        ComputerSearchLimits limits,
        CancellationToken cancellationToken = default);
}
