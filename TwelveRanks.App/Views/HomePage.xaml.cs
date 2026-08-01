using System;
using TwelveRanks.App.Services;
using TwelveRanks.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TwelveRanks.App.Views;

internal sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        Loaded += HomePage_Loaded;
    }

    private async void HomePage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        ResumeGameButton.Visibility =
            await GameSaveService.TryLoadAsync().ConfigureAwait(true) is null
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    private void ResumeGameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        ((ChessApplication)Application.Current)
            .MainWindow
            .NavigateToSavedGame();
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is string modeName &&
            System.Enum.TryParse(modeName, out GameMode mode))
        {
            ((ChessApplication)Application.Current).MainWindow.NavigateToGame(mode);
        }
    }
}
