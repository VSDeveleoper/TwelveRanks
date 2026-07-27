using Chess12x.Core;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Chess12x.App.Views;

public sealed partial class GamePage : Page
{
    private static readonly SolidColorBrush LightSquareBrush =
        new(ColorHelper.FromArgb(255, 238, 217, 181));

    private static readonly SolidColorBrush DarkSquareBrush =
        new(ColorHelper.FromArgb(255, 108, 142, 87));

    private static readonly SolidColorBrush GridLineBrush =
        new(ColorHelper.FromArgb(70, 40, 34, 27));

    public GamePage()
    {
        InitializeComponent();
        CreateEmptyBoard();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ModeText.Text = e.Parameter is GameMode mode
            ? mode switch
            {
                GameMode.LocalTwoPlayer => "Local two-player game",
                GameMode.Computer => "Game against the computer",
                GameMode.AI => "Game against AI",
                _ => "New game"
            }
            : "Choose a game mode from Home.";
    }

    private void CreateEmptyBoard()
    {
        for (var index = 0; index < BoardSpecification.Ranks; index++)
        {
            BoardGrid.RowDefinitions.Add(new RowDefinition());
        }

        for (var index = 0; index < BoardSpecification.Files; index++)
        {
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (var rank = 0; rank < BoardSpecification.Ranks; rank++)
        {
            for (var file = 0; file < BoardSpecification.Files; file++)
            {
                var square = new Border
                {
                    Background = (rank + file) % 2 == 0
                        ? LightSquareBrush
                        : DarkSquareBrush,
                    BorderBrush = GridLineBrush,
                    BorderThickness = new Thickness(0.5)
                };

                AutomationProperties.SetName(square, $"Board square {file + 1}, {rank + 1}");
                Grid.SetRow(square, rank);
                Grid.SetColumn(square, file);
                BoardGrid.Children.Add(square);
            }
        }
    }
}
