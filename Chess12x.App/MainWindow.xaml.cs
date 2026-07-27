using Chess12x.App.Views;
using Chess12x.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Chess12x.App;

public sealed partial class MainWindow : Window
{
    private bool _suppressNavigationSelection;

    public MainWindow()
    {
        InitializeComponent();
        NavigateFromTopBar(HomeNavigationItem, typeof(HomePage));
    }

    public void NavigateToGame(GameMode mode)
    {
        _suppressNavigationSelection = true;
        TopNavigation.SelectedItem = GameNavigationItem;
        _suppressNavigationSelection = false;
        ContentFrame.Navigate(typeof(GamePage), mode);
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
                Navigate(typeof(GamePage));
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
        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
