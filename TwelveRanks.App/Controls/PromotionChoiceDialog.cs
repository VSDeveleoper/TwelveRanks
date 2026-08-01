using System;
using System.Threading.Tasks;
using TwelveRanks.App.Services;
using TwelveRanks.Core;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace TwelveRanks.App.Controls;

internal sealed class PromotionChoiceDialog
{
    private readonly GridView _promotionChoices;
    private readonly ContentDialog _dialog;

    public PromotionChoiceDialog(
        XamlRoot xamlRoot,
        ChessMove[] promotionMoves,
        PieceSide promotionSide)
    {
        ArgumentNullException.ThrowIfNull(xamlRoot);
        ArgumentNullException.ThrowIfNull(promotionMoves);

        _promotionChoices = CreatePromotionChoices(
            promotionMoves,
            promotionSide);
        _dialog = CreateDialog(xamlRoot, _promotionChoices);
    }

    public async Task<ChessMove?> ShowAsync()
    {
        ContentDialogResult result =
            await _dialog.ShowAsync();

        return result == ContentDialogResult.Primary &&
            _promotionChoices.SelectedItem is GridViewItem
            {
                Tag: ChessMove selectedMove
            }
                ? selectedMove
                : null;
    }

    private static GridView CreatePromotionChoices(
        ChessMove[] promotionMoves,
        PieceSide promotionSide)
    {
        GridView choices = new()
        {
            FlowDirection = AppLanguageService.FlowDirection,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            SelectionMode = ListViewSelectionMode.Single,
            SingleSelectionFollowsFocus = true
        };
        AutomationProperties.SetAutomationId(
            choices,
            "PromotionChoice");
        AutomationProperties.SetName(
            choices,
            LocalizationResources.GetString(
                "PromotionChoicesAutomationName"));

        foreach (ChessMove move in promotionMoves)
        {
            if (move.PromotionPiece is PieceKind promotionPiece)
            {
                choices.Items.Add(
                    CreatePromotionOption(
                        move,
                        promotionPiece,
                        promotionSide));
            }
        }

        choices.SelectedIndex = 0;
        return choices;
    }

    private static ContentDialog CreateDialog(
        XamlRoot xamlRoot,
        GridView promotionChoices)
    {
        StackPanel content = new()
        {
            Spacing = 8
        };
        content.Children.Add(new TextBlock
        {
            Text = LocalizationResources.GetString(
                "PromotionInstruction"),
            TextWrapping = TextWrapping.Wrap
        });
        content.Children.Add(promotionChoices);

        ContentDialog dialog = new()
        {
            XamlRoot = xamlRoot,
            FlowDirection = AppLanguageService.FlowDirection,
            Title = LocalizationResources.GetString(
                "PromotionDialogTitle"),
            Content = content,
            PrimaryButtonText = LocalizationResources.GetString(
                "PromotionConfirm"),
            CloseButtonText = LocalizationResources.GetString(
                "PromotionCancel"),
            DefaultButton = ContentDialogButton.Primary
        };
        AutomationProperties.SetAutomationId(
            dialog,
            "PromotionDialog");
        AutomationProperties.SetName(
            dialog,
            LocalizationResources.GetString(
                "PromotionDialogAutomationName"));

        dialog.Opened += (_, _) =>
        {
            if (promotionChoices.SelectedItem is
                GridViewItem selectedItem)
            {
                _ = selectedItem.Focus(FocusState.Programmatic);
            }
            else
            {
                _ = promotionChoices.Focus(FocusState.Programmatic);
            }
        };

        return dialog;
    }

    private static GridViewItem CreatePromotionOption(
        ChessMove move,
        PieceKind promotionPiece,
        PieceSide promotionSide)
    {
        string label = ChessText.GetPromotionPieceName(
            promotionPiece);
        StackPanel content = CreateTileContent(
            promotionPiece,
            promotionSide,
            label);

        GridViewItem option = new()
        {
            Content = content,
            Tag = move,
            MinWidth = 104,
            MinHeight = 112,
            Margin = new Thickness(4),
            Padding = new Thickness(8),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        AutomationProperties.SetAutomationId(
            option,
            $"PromotionOption_{promotionPiece}");
        AutomationProperties.SetName(
            option,
            LocalizationResources.Format(
                "PromotionOptionAutomationNameFormat",
                label));
        AutomationProperties.SetHelpText(
            option,
            LocalizationResources.Format(
                "PromotionOptionHelpTextFormat",
                label));

        return option;
    }

    private static StackPanel CreateTileContent(
        PieceKind promotionPiece,
        PieceSide promotionSide,
        string label)
    {
        ChessPieceView artwork = new(
            new BoardPiece(promotionPiece, promotionSide))
        {
            Width = 72,
            Height = 72,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsHitTestVisible = false
        };
        TextBlock labelText = new()
        {
            Text = label,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        StackPanel content = new()
        {
            Width = 88,
            Spacing = 4
        };
        content.Children.Add(artwork);
        content.Children.Add(labelText);
        return content;
    }

}
