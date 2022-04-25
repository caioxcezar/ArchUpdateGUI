using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UpdaterGUI.Views;

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