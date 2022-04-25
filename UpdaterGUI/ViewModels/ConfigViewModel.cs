using System;
using System.Linq;
using System.Reflection;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using UpdaterGUI.Backend;

namespace UpdaterGUI.ViewModels;

public class ConfigViewModel : ViewModelBase
{
    public ConfigViewModel(MainWindowViewModel main) : base(main) { }

    public void ClearCache()
    {
        try
        {
            var assembly = Assembly.GetAssembly(GetType());
            var classes = assembly!.GetTypes().Where(t => t.IsClass && t.IsAssignableTo(typeof(IProvider)));
            foreach (var @class in classes)
            {
                var obj = (IProvider) Activator.CreateInstance(@class)!;
                Cache.Clear(obj.Name);
            }
            MessageBoxManager.GetMessageBoxStandardWindow("Finished", "The cache has been cleared. ", ButtonEnum.Ok, Icon.Success).Show();
        }
        catch (Exception e)
        {
            MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", e.Message, ButtonEnum.Ok, Icon.Error).Show();
        }
    }

    public void Back() => GoBack();
}