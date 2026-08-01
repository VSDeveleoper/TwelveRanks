using System;
using Microsoft.UI.Xaml;
using Windows.Globalization;
using Windows.Storage;

namespace TwelveRanks.App.Services;

internal static class AppLanguageService
{
    internal const string SystemLanguage = "system";
    internal const string EnglishLanguage = "en-US";
    internal const string ArabicLanguage = "ar";
    internal const string SpanishLanguage = "es";
    internal const string FrenchLanguage = "fr";
    internal const string GermanLanguage = "de";
    internal const string PortugueseBrazilLanguage = "pt-BR";
    internal const string ChineseSimplifiedLanguage = "zh-Hans";
    internal const string ChineseTraditionalLanguage = "zh-Hant";
    internal const string JapaneseLanguage = "ja";
    internal const string KoreanLanguage = "ko";
    internal const string ItalianLanguage = "it";
    internal const string PolishLanguage = "pl";
    internal const string TurkishLanguage = "tr";
    internal const string HindiLanguage = "hi";
    internal const string DutchLanguage = "nl";
    internal const string IndonesianLanguage = "id";
    internal const string VietnameseLanguage = "vi";
    internal const string UkrainianLanguage = "uk";

    private const string PendingLanguageOverrideKey =
        "PendingLanguageOverride";

    private static readonly string[] SupportedLanguageOverrides =
    [
        EnglishLanguage,
        ArabicLanguage,
        SpanishLanguage,
        FrenchLanguage,
        GermanLanguage,
        PortugueseBrazilLanguage,
        ChineseSimplifiedLanguage,
        ChineseTraditionalLanguage,
        JapaneseLanguage,
        KoreanLanguage,
        ItalianLanguage,
        PolishLanguage,
        TurkishLanguage,
        HindiLanguage,
        DutchLanguage,
        IndonesianLanguage,
        VietnameseLanguage,
        UkrainianLanguage
    ];

    internal static FlowDirection FlowDirection
    {
        get
        {
            string languageTag = ApplicationLanguages.Languages.Count > 0
                ? ApplicationLanguages.Languages[0]
                : EnglishLanguage;
            Language language = new(languageTag);

            return language.LayoutDirection ==
                LanguageLayoutDirection.Rtl
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
        }
    }

    internal static void ApplyPendingLanguageOverride()
    {
        if (!TryGetPendingLanguage(out string selection))
        {
            return;
        }

        ApplicationLanguages.PrimaryLanguageOverride =
            GetOverrideLanguage(selection);
        _ = ApplicationData.Current.LocalSettings.Values.Remove(
            PendingLanguageOverrideKey);
    }

    internal static string GetSelectedLanguage()
    {
        return TryGetPendingLanguage(out string selection)
            ? selection
            : NormalizeOverride(
                ApplicationLanguages.PrimaryLanguageOverride);
    }

    internal static bool HasPendingLanguageChange()
    {
        return TryGetPendingLanguage(out _);
    }

    internal static void StageLanguage(string selection)
    {
        if (!IsSupportedSelection(selection))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selection),
                selection,
                "Unsupported application language selection.");
        }

        string currentSelection = NormalizeOverride(
            ApplicationLanguages.PrimaryLanguageOverride);
        if (string.Equals(
                selection,
                currentSelection,
                StringComparison.OrdinalIgnoreCase))
        {
            _ = ApplicationData.Current.LocalSettings.Values.Remove(
                PendingLanguageOverrideKey);
            return;
        }

        ApplicationData.Current.LocalSettings.Values[
            PendingLanguageOverrideKey] = selection;
    }

    private static bool TryGetPendingLanguage(out string selection)
    {
        if (ApplicationData.Current.LocalSettings.Values.TryGetValue(
                PendingLanguageOverrideKey,
                out object? storedValue) &&
            storedValue is string storedSelection &&
            IsSupportedSelection(storedSelection))
        {
            selection = storedSelection;
            return true;
        }

        _ = ApplicationData.Current.LocalSettings.Values.Remove(
            PendingLanguageOverrideKey);
        selection = SystemLanguage;
        return false;
    }

    private static string NormalizeOverride(string languageTag)
    {
        if (string.IsNullOrEmpty(languageTag))
        {
            return SystemLanguage;
        }

        foreach (string supportedLanguage in
            SupportedLanguageOverrides)
        {
            if (IsSameLanguage(languageTag, supportedLanguage))
            {
                return supportedLanguage;
            }
        }

        return SystemLanguage;
    }

    private static string GetOverrideLanguage(string selection)
    {
        return string.Equals(
            selection,
            SystemLanguage,
            StringComparison.Ordinal)
                ? string.Empty
                : selection;
    }

    private static bool IsSupportedSelection(string selection)
    {
        if (string.Equals(
                selection,
                SystemLanguage,
                StringComparison.Ordinal))
        {
            return true;
        }

        foreach (string supportedLanguage in
            SupportedLanguageOverrides)
        {
            if (string.Equals(
                    selection,
                    supportedLanguage,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSameLanguage(
        string languageTag,
        string supportedLanguage)
    {
        return string.Equals(
                languageTag,
                supportedLanguage,
                StringComparison.OrdinalIgnoreCase) ||
            languageTag.StartsWith(
                $"{supportedLanguage}-",
                StringComparison.OrdinalIgnoreCase) ||
            supportedLanguage.StartsWith(
                $"{languageTag}-",
                StringComparison.OrdinalIgnoreCase);
    }
}
