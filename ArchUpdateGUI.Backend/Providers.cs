using System.Reflection;

namespace ArchUpdateGUI.Backend;

public class Providers
{
    public List<IProvider> List { get; }

    public Providers()
    {
        List = new();
        var assembly = Assembly.GetAssembly(GetType());
        var classes = assembly!.GetTypes().Where(t => t.IsClass && t.IsAssignableTo(typeof(IProvider)));
        foreach (var @class in classes)
        {
            var obj = (IProvider)Activator.CreateInstance(@class)!;
            if(obj.Version().ExitCode == 0) List.Add(obj);
        }
    }
}