using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ArchUpdateGUI.Models;

public static class Cache
{
    public static Task Save(this string[] lines, string name) => File.WriteAllLinesAsync(name, lines, Encoding.UTF8);

    public static Task<string[]> Load(string name) => File.ReadAllLinesAsync(name, Encoding.UTF8);
}