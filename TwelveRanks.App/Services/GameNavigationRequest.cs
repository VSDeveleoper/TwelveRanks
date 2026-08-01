using TwelveRanks.Core;

namespace TwelveRanks.App.Services;

internal readonly record struct GameNavigationRequest(
    GameMode? RequestedMode,
    bool ResumeSavedGame)
{
    internal static GameNavigationRequest ChooseMode => new(null, false);

    internal static GameNavigationRequest Resume => new(null, true);

    internal static GameNavigationRequest StartNew(GameMode mode)
    {
        return new GameNavigationRequest(mode, false);
    }
}
