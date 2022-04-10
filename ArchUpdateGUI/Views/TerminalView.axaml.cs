using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArchUpdateGUI.Views;

public partial class TerminalView : UserControl
{
    public TerminalView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}