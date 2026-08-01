using Windows.Storage;

namespace TwelveRanks.App.Services;

internal static class AppSettingsService
{
    private const string ShowBoardCoordinatesKey =
        "ShowBoardCoordinates";
    private const string PlayMoveSoundsKey =
        "PlayMoveSounds";
    private const string ConfirmBeforeResigningKey =
        "ConfirmBeforeResigning";

    internal static bool ShowBoardCoordinates
    {
        get => GetBoolean(ShowBoardCoordinatesKey, true);
        set => ApplicationData.Current.LocalSettings.Values[
            ShowBoardCoordinatesKey] = value;
    }

    internal static bool PlayMoveSounds
    {
        get => GetBoolean(PlayMoveSoundsKey, true);
        set => ApplicationData.Current.LocalSettings.Values[
            PlayMoveSoundsKey] = value;
    }

    internal static bool ConfirmBeforeResigning
    {
        get => GetBoolean(ConfirmBeforeResigningKey, true);
        set => ApplicationData.Current.LocalSettings.Values[
            ConfirmBeforeResigningKey] = value;
    }

    private static bool GetBoolean(
        string key,
        bool defaultValue)
    {
        return ApplicationData.Current.LocalSettings.Values.TryGetValue(
                key,
                out object? storedValue) &&
            storedValue is bool value
                ? value
                : defaultValue;
    }
}
