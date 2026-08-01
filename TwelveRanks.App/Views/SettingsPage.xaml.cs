using System.Collections.Generic;
using System.Globalization;
using TwelveRanks.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace TwelveRanks.App.Views;

internal sealed partial class SettingsPage : Page
{
    private bool _isInitializingLanguageSelection = true;
    private bool _isInitializingPreferences = true;

    public SettingsPage()
    {
        CurrentVersion = GetCurrentVersion();
        ReleaseNotes = CreateReleaseNotes(CurrentVersion);
        InitializeComponent();
        InitializeLanguageSelection();
        InitializePreferences();
    }

    public string CurrentVersion { get; }

    public IReadOnlyList<SettingsReleaseNote> ReleaseNotes { get; }

    private static IReadOnlyList<SettingsReleaseNote> CreateReleaseNotes(
        string currentVersion)
    {
        return
        [
            new SettingsReleaseNote(
                currentVersion,
                LocalizationResources.GetString(
                    "ReleaseNoteSummary"),
                [
                    LocalizationResources.GetString(
                        "ReleaseNoteItem1"),
                    LocalizationResources.GetString(
                        "ReleaseNoteItem2"),
                    LocalizationResources.GetString(
                        "ReleaseNoteItem3"),
                    LocalizationResources.GetString(
                        "ReleaseNoteItem4"),
                    LocalizationResources.GetString(
                        "ReleaseNoteItem5"),
                    LocalizationResources.GetString(
                        "ReleaseNoteItem6")
                ])
        ];
    }

    private void InitializeLanguageSelection()
    {
        string selectedLanguage =
            AppLanguageService.GetSelectedLanguage();

        foreach (object item in ApplicationLanguageComboBox.Items)
        {
            if (item is ComboBoxItem
                {
                    Tag: string language
                } option &&
                string.Equals(
                    language,
                    selectedLanguage,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                ApplicationLanguageComboBox.SelectedItem = option;
                break;
            }
        }

        LanguageRestartInfoBar.IsOpen =
            AppLanguageService.HasPendingLanguageChange();
        _isInitializingLanguageSelection = false;
    }

    private void ApplicationLanguageComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        System.ArgumentNullException.ThrowIfNull(sender);
        System.ArgumentNullException.ThrowIfNull(e);

        if (_isInitializingLanguageSelection ||
            ApplicationLanguageComboBox.SelectedItem is not ComboBoxItem
            {
                Tag: string selectedLanguage
            })
        {
            return;
        }

        AppLanguageService.StageLanguage(selectedLanguage);
        LanguageRestartInfoBar.Severity =
            InfoBarSeverity.Informational;
        LanguageRestartInfoBar.Title =
            LocalizationResources.GetString(
                "LanguageRestartTitle");
        LanguageRestartInfoBar.Message =
            LocalizationResources.GetString(
                "LanguageRestartMessage");
        LanguageRestartInfoBar.IsOpen =
            AppLanguageService.HasPendingLanguageChange();
    }

    private void InitializePreferences()
    {
        ShowBoardCoordinatesToggle.IsOn =
            AppSettingsService.ShowBoardCoordinates;
        PlayMoveSoundsToggle.IsOn =
            AppSettingsService.PlayMoveSounds;
        ConfirmBeforeResigningToggle.IsOn =
            AppSettingsService.ConfirmBeforeResigning;
        _isInitializingPreferences = false;
    }

    private void ShowBoardCoordinatesToggle_Toggled(
        object sender,
        RoutedEventArgs e)
    {
        System.ArgumentNullException.ThrowIfNull(sender);
        System.ArgumentNullException.ThrowIfNull(e);

        if (!_isInitializingPreferences)
        {
            AppSettingsService.ShowBoardCoordinates =
                ShowBoardCoordinatesToggle.IsOn;
        }
    }

    private void PlayMoveSoundsToggle_Toggled(
        object sender,
        RoutedEventArgs e)
    {
        System.ArgumentNullException.ThrowIfNull(sender);
        System.ArgumentNullException.ThrowIfNull(e);

        if (!_isInitializingPreferences)
        {
            AppSettingsService.PlayMoveSounds =
                PlayMoveSoundsToggle.IsOn;
        }
    }

    private void ConfirmBeforeResigningToggle_Toggled(
        object sender,
        RoutedEventArgs e)
    {
        System.ArgumentNullException.ThrowIfNull(sender);
        System.ArgumentNullException.ThrowIfNull(e);

        if (!_isInitializingPreferences)
        {
            AppSettingsService.ConfirmBeforeResigning =
                ConfirmBeforeResigningToggle.IsOn;
        }
    }

    private void RestartApplicationButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        System.ArgumentNullException.ThrowIfNull(sender);
        System.ArgumentNullException.ThrowIfNull(e);

        object failureReason =
            Microsoft.Windows.AppLifecycle.AppInstance.Restart(
                string.Empty);

        LanguageRestartInfoBar.Severity = InfoBarSeverity.Error;
        LanguageRestartInfoBar.Title =
            LocalizationResources.GetString(
                "LanguageRestartFailedTitle");
        LanguageRestartInfoBar.Message =
            LocalizationResources.Format(
                "LanguageRestartFailedMessageFormat",
                failureReason);
        LanguageRestartInfoBar.IsOpen = true;
    }

    private static string GetCurrentVersion()
    {
        PackageVersion version = Package.Current.Id.Version;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
    }
}

internal sealed record SettingsReleaseNote(
    string Version,
    string Summary,
    IReadOnlyList<string> Items);
