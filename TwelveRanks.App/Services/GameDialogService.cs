using System;
using System.Threading.Tasks;
using TwelveRanks.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace TwelveRanks.App.Services;

internal static class GameDialogService
{
    internal static async Task<GameMode?> ShowGameModeAsync(
        XamlRoot xamlRoot)
    {
        string title =
            LocalizationResources.GetString("ChooseGameModeDialogTitle");
        ContentDialog dialog = CreateDialog(
            xamlRoot,
            title,
            "ChooseGameModeDialog");
        dialog.Content = LocalizationResources.GetString(
            "ChooseGameModeDialogMessage");
        dialog.PrimaryButtonText = LocalizationResources.GetString(
            "ChooseGameModeDialogLocalButton");
        dialog.SecondaryButtonText = LocalizationResources.GetString(
            "ChooseGameModeDialogComputerButton");
        dialog.CloseButtonText = LocalizationResources.GetString(
            "GameDialogCancelButton");
        dialog.DefaultButton = ContentDialogButton.Primary;

        return await dialog.ShowAsync() switch
        {
            ContentDialogResult.Primary => GameMode.LocalTwoPlayer,
            ContentDialogResult.Secondary => GameMode.Computer,
            _ => null
        };
    }

    internal static async Task<bool?> ShowResumeGameAsync(
        XamlRoot xamlRoot)
    {
        string title =
            LocalizationResources.GetString("ResumeGameDialogTitle");
        ContentDialog dialog = CreateDialog(
            xamlRoot,
            title,
            "ResumeGameDialog");
        dialog.Content = LocalizationResources.GetString(
            "ResumeGameDialogMessage");
        dialog.PrimaryButtonText = LocalizationResources.GetString(
            "ResumeGameDialogResumeButton");
        dialog.SecondaryButtonText = LocalizationResources.GetString(
            "ResumeGameDialogNewGameButton");
        dialog.CloseButtonText = LocalizationResources.GetString(
            "GameDialogCancelButton");
        dialog.DefaultButton = ContentDialogButton.Primary;

        return await dialog.ShowAsync() switch
        {
            ContentDialogResult.Primary => true,
            ContentDialogResult.Secondary => false,
            _ => null
        };
    }

    internal static async Task ShowSaveUnavailableAsync(
        XamlRoot xamlRoot)
    {
        string title =
            LocalizationResources.GetString("GameSaveUnavailableTitle");
        ContentDialog dialog = CreateDialog(
            xamlRoot,
            title,
            "GameSaveUnavailableDialog");
        dialog.Content = LocalizationResources.GetString(
            "GameSaveUnavailableMessage");
        dialog.CloseButtonText = LocalizationResources.GetString(
            "GameDialogCloseButton");
        dialog.DefaultButton = ContentDialogButton.Close;
        _ = await dialog.ShowAsync();
    }

    internal static async Task<bool> ShowResignConfirmationAsync(
        XamlRoot xamlRoot,
        PieceSide resignedSide)
    {
        PieceSide winner = resignedSide == PieceSide.White
            ? PieceSide.Black
            : PieceSide.White;
        string title =
            LocalizationResources.GetString("ResignDialogTitle");
        ContentDialog dialog = CreateDialog(
            xamlRoot,
            title,
            "ResignDialog");
        dialog.Content = LocalizationResources.Format(
            "ResignDialogMessageFormat",
            ChessText.GetSideName(resignedSide),
            ChessText.GetSideName(winner));
        dialog.PrimaryButtonText = LocalizationResources.GetString(
            "ResignDialogConfirmButton");
        dialog.CloseButtonText = LocalizationResources.GetString(
            "GameDialogCancelButton");
        dialog.DefaultButton = ContentDialogButton.Close;

        return await dialog.ShowAsync() ==
            ContentDialogResult.Primary;
    }

    internal static async Task<bool> ShowEndGameConfirmationAsync(
        XamlRoot xamlRoot)
    {
        string title =
            LocalizationResources.GetString("EndGameDialogTitle");
        ContentDialog dialog = CreateDialog(
            xamlRoot,
            title,
            "EndGameDialog");
        dialog.Content = LocalizationResources.GetString(
            "EndGameDialogMessage");
        dialog.PrimaryButtonText = LocalizationResources.GetString(
            "EndGameDialogConfirmButton");
        dialog.CloseButtonText = LocalizationResources.GetString(
            "GameDialogCancelButton");
        dialog.DefaultButton = ContentDialogButton.Close;

        return await dialog.ShowAsync() ==
            ContentDialogResult.Primary;
    }

    internal static async Task ShowEndGameUnavailableAsync(
        XamlRoot xamlRoot)
    {
        string title =
            LocalizationResources.GetString(
                "EndGameUnavailableTitle");
        ContentDialog dialog = CreateDialog(
            xamlRoot,
            title,
            "EndGameUnavailableDialog");
        dialog.Content = LocalizationResources.GetString(
            "EndGameUnavailableMessage");
        dialog.CloseButtonText = LocalizationResources.GetString(
            "GameDialogCloseButton");
        dialog.DefaultButton = ContentDialogButton.Close;
        _ = await dialog.ShowAsync();
    }

    private static ContentDialog CreateDialog(
        XamlRoot xamlRoot,
        string title,
        string automationId)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = xamlRoot,
            FlowDirection = AppLanguageService.FlowDirection,
            Title = title
        };
        AutomationProperties.SetAutomationId(
            dialog,
            automationId);
        AutomationProperties.SetName(dialog, title);
        return dialog;
    }
}
