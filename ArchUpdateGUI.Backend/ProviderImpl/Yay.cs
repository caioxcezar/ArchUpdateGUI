using System.Security;

namespace ArchUpdateGUI.Backend.ProviderImpl;

internal class Yay : IProvider
{
    public string Name => "Yay - AUR Helper";
    public bool RootRequired => true;
    public List<Package> Packages { get; }
    public int Installed { get; private set; }
    public int Total { get; private set; }

    public Yay()
    {
        Packages = new();
    }
    public void Load(bool cached)
    {
        Packages.Clear();
        List<string> list = new();
        if (cached)
        {
            try
            {
                list.AddRange(Cache.Load(Name).Result);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(AggregateException) &&
                    e.InnerException!.GetType() == typeof(FileNotFoundException))
                    list.AddRange(LoadUncached().Result);
                else throw;
            }
        }
        else list.AddRange(LoadUncached().Result);

        foreach (var package in list)
        {
            string[] listPackage = package.Split(' ');
            if (listPackage.Length > 1)
                Packages.Add(new()
                {
                    Provider = "yay",
                    Repository = listPackage[0],
                    Name = listPackage[1],
                    Version = listPackage[2],
                    IsInstalled = listPackage.Length == 4
                });
        }
        Installed = Packages.Count(p => p.IsInstalled);
        Total = Packages.Count;
    }

    private async Task<string[]> LoadUncached()
    {
        var result = Command.Run("yay -Sl");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        var output = result.Output.Split('\n');
        await output.Save(Name);
        return output;
    }
    public string PackageInfo(Package package)
    {
        var result = Command.Run($"yay -Si {package.Name}");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        return result.Output;
    }

    public Task<int> Install(SecureString? pass, IList<Package> packages, Action<string?> output, Action<string?> error) =>
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S su && yay -Syu {string.Join(' ', packages.Select(p => p.Name))} --noconfirm", output, error);

    public Task<int> Remove(SecureString? pass, IList<Package> packages, Action<string?> output, Action<string?> error) =>
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S su && yay -Rsu {string.Join(' ', packages.Select(p => p.Name))} --noconfirm", output, error);

    public Task<int> Update(SecureString? pass, Action<string?> output, Action<string?> error) =>
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S su && yay -Syu --noconfirm", output, error);

    public Command Version() => Command.Run("yay --version");
}