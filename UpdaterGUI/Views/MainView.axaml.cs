using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData.Kernel;
using UpdaterGUI.Backend;
using UpdaterGUI.ViewModels;

namespace UpdaterGUI.Views;

public partial class MainView : UserControl
{
    private DataGrid? _dataGridPackages;
    private Package? _oldValue;

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _dataGridPackages = this.FindControl<DataGrid>("DataGridPackages");
        _dataGridPackages.BeginningEdit += (_, args) => _oldValue = ((Package) args.Row.DataContext!).Clone();
        _dataGridPackages.CellEditEnded += DataGrid_EventHandler;
    }

    private void DataGrid_EventHandler(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext == null || MainViewModel.ChangedPackages == null || _oldValue == null) return;
        if (MainViewModel.ChangedPackages == null) throw new Exception("ChangedPackages not initiated wet. ");
        var cell = (Package) e.Row.DataContext;
        if (cell.IsInstalled == _oldValue.IsInstalled) return;
        var value = MainViewModel.ChangedPackages.FirstOrOptional(p =>
            p.QualifiedName == cell.QualifiedName && p.Name == cell.Name);
        if (value.HasValue) MainViewModel.ChangedPackages.Remove(value.Value);
        else MainViewModel.ChangedPackages.Add(cell.Clone());
    }
}