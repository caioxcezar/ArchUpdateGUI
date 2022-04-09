using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ArchUpdateGUI.Views;

public partial class PasswordWindow : Window
{
    public PasswordWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}