using System;
using System.Diagnostics.CodeAnalysis;

using TwelveRanks.App.Services;
using Microsoft.UI.Xaml;

namespace TwelveRanks.App;

/// <summary>
/// Provides application startup and access to the main window.
/// </summary>
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "The WinUI XAML compiler generates a public IXamlMetadataProvider partial declaration for the application type.")]
public partial class ChessApplication : Application
{
    private MainWindow? _mainWindow;

    /// <summary>
    /// Gets the application's active main window.
    /// </summary>
    internal MainWindow MainWindow =>
        _mainWindow ??
        throw new InvalidOperationException(
            "The main window has not been created.");

    /// <summary>
    /// Initializes the Twelve Ranks application.
    /// </summary>
    public ChessApplication()
    {
        AppLanguageService.ApplyPendingLanguageOverride();
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }

    /// <summary>
    /// Brings the existing main window to the foreground after a
    /// redirected activation.
    /// </summary>
    internal void ActivateMainWindow()
    {
        _mainWindow?.Activate();
    }
}
