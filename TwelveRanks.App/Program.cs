using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace TwelveRanks.App;

internal static class Program
{
    private const string MainInstanceKey = "TwelveRanks.MainInstance";

    private static DispatcherQueue? _dispatcherQueue;

    [System.STAThread]
    private static async Task Main()
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();

        AppInstance currentInstance = AppInstance.GetCurrent();
        AppActivationArguments activationArguments =
            currentInstance.GetActivatedEventArgs();
        AppInstance mainInstance =
            AppInstance.FindOrRegisterForKey(MainInstanceKey);

        if (!mainInstance.IsCurrent)
        {
            await mainInstance.RedirectActivationToAsync(
                activationArguments);
            return;
        }

        mainInstance.Activated += MainInstance_Activated;

        Application.Start(initializationCallbackParams =>
        {
            _ = initializationCallbackParams;
            DispatcherQueue dispatcherQueue =
                DispatcherQueue.GetForCurrentThread();
            _dispatcherQueue = dispatcherQueue;
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherQueueSynchronizationContext(
                    dispatcherQueue));
            _ = new ChessApplication();
        });
    }

    private static void MainInstance_Activated(
        object? sender,
        AppActivationArguments args)
    {
        _ = sender;
        _ = args;
        _ = _dispatcherQueue?.TryEnqueue(ActivateMainWindow);
    }

    private static void ActivateMainWindow()
    {
        if (Application.Current is ChessApplication application)
        {
            application.ActivateMainWindow();
        }
    }
}
