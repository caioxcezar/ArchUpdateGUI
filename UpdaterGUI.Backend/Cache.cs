using System.Reflection;
using System.Text;

namespace UpdaterGUI.Backend;

public static class Cache
{
    private static string CachePath => Path.GetTempPath();
    private static string ProjectName => Assembly.GetCallingAssembly().GetName().Name ?? "UpdaterGUI";
    public static Task Save(this string[] lines, string name) => File.WriteAllLinesAsync($"{CachePath}{name}{ProjectName}", lines, Encoding.UTF8);
    public static Task<string[]> Load(string name) => File.ReadAllLinesAsync($"{CachePath}{name}{ProjectName}", Encoding.UTF8);
    public static void Clear(string name) => File.Delete($"{CachePath}{name}{ProjectName}");
}