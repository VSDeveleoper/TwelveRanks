using System;
using System.Globalization;
using Windows.ApplicationModel.Resources;

namespace TwelveRanks.App.Services;

internal static class LocalizationResources
{
    private static readonly ResourceLoader Loader =
        ResourceLoader.GetForViewIndependentUse();

    internal static string GetString(string resourceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);
        return Loader.GetString(resourceKey);
    }

    internal static string Format(
        string resourceKey,
        params object?[] arguments)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            GetString(resourceKey),
            arguments);
    }

    internal static string IsolateBidirectionalText(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return $"\u2068{value}\u2069";
    }
}
