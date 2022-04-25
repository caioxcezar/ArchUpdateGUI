using System;
using ReactiveUI;

namespace UpdaterGUI.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private MainWindowViewModel _main;

    public ViewModelBase(MainWindowViewModel main)
    {
        _main = main;
    }
    public void GoBack() => _main.GoBack();
    public void Navigate(Type viewModelBase, params object?[]? args) => _main.Navigate(viewModelBase, args);
    public void ShowLoading(bool isLoading) => _main.Loading = isLoading;
}