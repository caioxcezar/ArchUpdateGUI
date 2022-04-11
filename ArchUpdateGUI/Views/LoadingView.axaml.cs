using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArchUpdateGUI.Views;

public partial class LoadingView : UserControl
{
    public LoadingView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}