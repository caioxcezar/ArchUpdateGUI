using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace ArchUpdateGUI.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private MainWindowViewModel _main;

    public ViewModelBase(MainWindowViewModel main)
    {
        _main = main;
    }
    public void GoBack() => _main.GoBack();
    public void Navigate(Type viewModelBase, params object?[]? args) => _main.Navigate(viewModelBase, args);
}