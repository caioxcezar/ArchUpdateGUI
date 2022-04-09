using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArchUpdateGUI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}