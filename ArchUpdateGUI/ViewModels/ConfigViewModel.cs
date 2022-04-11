using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using JetBrains.Annotations;

namespace ArchUpdateGUI.ViewModels;

public class ConfigViewModel : ViewModelBase
{
    public ConfigViewModel(MainWindowViewModel main) : base(main) { }

    public void Back() => GoBack();
}