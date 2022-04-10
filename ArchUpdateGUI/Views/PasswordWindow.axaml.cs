using ArchUpdateGUI.ViewModels;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;

namespace ArchUpdateGUI.Views;

public partial class PasswordWindow : ReactiveWindow<PasswordViewModel>
{
    public PasswordWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        this.WhenActivated(d => d(ViewModel!.Ok.Subscribe(Close)));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}