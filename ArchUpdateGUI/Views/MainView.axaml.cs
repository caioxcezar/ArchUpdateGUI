using ArchUpdateGUI.Models;
using ArchUpdateGUI.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData.Kernel;

namespace ArchUpdateGUI.Views;

public partial class MainView : UserControl
{
    private DataGrid _dataGridPackages;
    private Package? OldValue;
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _dataGridPackages = this.FindControl<DataGrid>("DataGridPackages");
        _dataGridPackages.BeginningEdit += (_, args) => OldValue = ((Package)args.Row.DataContext!).Clone();
        _dataGridPackages.CellEditEnded += DataGrid_EventHandler;
    }

    private void DataGrid_EventHandler(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext == null || MainViewModel.ChangedPackages == null || OldValue == null) return;
        var cell = (Package) e.Row.DataContext;
        if (cell.IsInstalled == OldValue.IsInstalled) return;
        if (MainViewModel.ChangedPackages
            .FirstOrOptional(p => p.QualifiedName == cell.QualifiedName || p.Name == cell.Name)
            .HasValue) MainViewModel.ChangedPackages.Remove(cell);
        else
            MainViewModel.ChangedPackages.Add(cell.Clone());
    }
}