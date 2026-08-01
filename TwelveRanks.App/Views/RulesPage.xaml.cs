using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;

namespace TwelveRanks.App.Views;

[SuppressMessage(
    "Maintainability",
    "CA1501:Avoid excessive inheritance",
    Justification = "A WinUI Page necessarily inherits through the framework control hierarchy.")]
internal sealed partial class RulesPage : Page
{
    public RulesPage()
    {
        InitializeComponent();
    }
}
