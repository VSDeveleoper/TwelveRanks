using Microsoft.UI.Xaml;

namespace Chess12x.App;

public partial class App : Application
{
    public MainWindow MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
