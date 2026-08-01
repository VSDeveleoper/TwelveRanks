using System;
using TwelveRanks.App.Views;
using TwelveRanks.App.Services;
using TwelveRanks.Core;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace TwelveRanks.App;

internal sealed partial class MainWindow : Window
{
    private bool _suppressNavigationSelection;
    private bool _isExitDialogOpen;
    private bool _isExitConfirmed;

    public MainWindow()
    {
        InitializeComponent();
        Title = LocalizationResources.GetString("AppDisplayName");
        TopNavigation.FlowDirection = AppLanguageService.FlowDirection;
        AppWindow.Closing += AppWindow_Closing;
        NavigateFromTopBar(HomeNavigationItem, typeof(HomePage));
    }

    public void NavigateToGame(GameMode mode)
    {
        NavigateToGame(GameNavigationRequest.StartNew(mode));
    }

    public void NavigateToSavedGame()
    {
        NavigateToGame(GameNavigationRequest.Resume);
    }

    public void NavigateToHome()
    {
        NavigateFromTopBar(HomeNavigationItem, typeof(HomePage));
    }

    internal void UpdateGameCommandState(
        bool isGamePageActive,
        bool canUndo,
        bool canRedo,
        bool canEndGame)
    {
        GameCommandBar.Visibility = isGamePageActive
            ? Visibility.Visible
            : Visibility.Collapsed;
        UndoButton.IsEnabled = isGamePageActive && canUndo;
        RedoButton.IsEnabled = isGamePageActive && canRedo;
        EndGameButton.IsEnabled =
            isGamePageActive && canEndGame;
    }

    private void NavigateToGame(GameNavigationRequest request)
    {
        _suppressNavigationSelection = true;
        TopNavigation.SelectedItem = GameNavigationItem;
        _suppressNavigationSelection = false;
        _ = ContentFrame.Navigate(typeof(GamePage), request);
    }

    private void TopNavigation_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (_suppressNavigationSelection ||
            args.SelectedItemContainer is not NavigationViewItem selectedItem)
        {
            return;
        }

        switch (selectedItem.Tag as string)
        {
            case "home":
                Navigate(typeof(HomePage));
                break;
            case "game":
                Navigate(
                    typeof(GamePage),
                    GameNavigationRequest.ChooseMode);
                break;
            case "rules":
                Navigate(typeof(RulesPage));
                break;
            case "settings":
                Navigate(typeof(SettingsPage));
                break;
        }
    }

    private void NavigateFromTopBar(NavigationViewItem item, System.Type pageType)
    {
        _suppressNavigationSelection = true;
        TopNavigation.SelectedItem = item;
        _suppressNavigationSelection = false;
        Navigate(pageType);
    }

    private void Navigate(System.Type pageType)
    {
        Navigate(pageType, null);
    }

    private void Navigate(System.Type pageType, object? parameter)
    {
        if (ContentFrame.CurrentSourcePageType != pageType ||
            parameter is not null)
        {
            _ = ContentFrame.Navigate(pageType, parameter);
        }
    }

    private async void UndoButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        if (ContentFrame.Content is GamePage gamePage)
        {
            await gamePage.UndoAsync().ConfigureAwait(true);
        }
    }

    private async void RedoButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        if (ContentFrame.Content is GamePage gamePage)
        {
            await gamePage.RedoAsync().ConfigureAwait(true);
        }
    }

    private async void EndGameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        if (ContentFrame.Content is GamePage gamePage)
        {
            await gamePage.EndGameAsync().ConfigureAwait(true);
        }
    }

    private async void AppWindow_Closing(
        AppWindow sender,
        AppWindowClosingEventArgs args)
    {
        if (_isExitConfirmed)
        {
            return;
        }

        args.Cancel = true;
        if (_isExitDialogOpen)
        {
            return;
        }

        _isExitDialogOpen = true;
        try
        {
            ContentDialog dialog = CreateExitConfirmationDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                _isExitConfirmed = true;
                Close();
            }
        }
        finally
        {
            _isExitDialogOpen = false;
        }
    }

    private ContentDialog CreateExitConfirmationDialog()
    {
        string title =
            LocalizationResources.GetString("ExitGameDialogTitle");
        ContentDialog dialog = new()
        {
            XamlRoot = TopNavigation.XamlRoot,
            FlowDirection = AppLanguageService.FlowDirection,
            Title = title,
            Content = LocalizationResources.GetString(
                "ExitGameDialogMessage"),
            PrimaryButtonText = LocalizationResources.GetString(
                "ExitGameDialogExitButton"),
            CloseButtonText = LocalizationResources.GetString(
                "ExitGameDialogCancelButton"),
            DefaultButton = ContentDialogButton.Close
        };
        AutomationProperties.SetAutomationId(
            dialog,
            "ExitGameDialog");
        AutomationProperties.SetName(dialog, title);
        return dialog;
    }
}
