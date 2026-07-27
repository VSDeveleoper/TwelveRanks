using Chess12x.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Chess12x.App.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is string modeName &&
            System.Enum.TryParse(modeName, out GameMode mode))
        {
            ((App)Application.Current).MainWindow.NavigateToGame(mode);
        }
    }
}
