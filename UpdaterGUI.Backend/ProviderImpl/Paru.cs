using System.Security;

namespace UpdaterGUI.Backend.ProviderImpl;

public class Paru : IProvider
{
    public Paru()
    {
        Packages = new();
    }

    public string Name => "Paru - AUR helper";
    public List<Package> Packages { get; }
    public int Installed { get; private set; }
    public int Total { get; private set; }
    public bool RootRequired => true;

    public void Load(bool cached)
    {
        Packages.Clear();
        var result = Command.Run("paru -Sl");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        Packages.AddRange(result.Output.Split("\n").Where(str => !string.IsNullOrWhiteSpace(str))
            .Select(package =>
            {
                var p = package.Split(' ');
                return new Package
                {
                    Provider = "paru",
                    Repository = p[0],
                    Name = p[1],
                    Version = p[2],
                    IsInstalled = p.Length == 4
                };
            }));
        Installed = Packages.Count(p => p.IsInstalled);
        Total = Packages.Count;
    }

    public Command Version() => Command.Run("paru --version");

    public string PackageInfo(Package package)
    {
        var result = Command.Run($"paru -Si {package.Name}");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        return result.Output;
    }

    public Task<int> Install(SecureString? pass, IList<Package> packages, Action<string?> output, Action<string?> error) => 
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S su && paru -Syu {string.Join(' ', packages.Select(p => p.Name))} --noconfirm", output, error);

    public Task<int> Remove(SecureString? pass, IList<Package> packages, Action<string?> output, Action<string?> error) => 
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S su && paru -Rsu {string.Join(' ', packages.Select(p => p.Name))} --noconfirm", output, error);

    public Task<int> Update(SecureString? pass, Action<string?> output, Action<string?> error) => 
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S su && paru -Syu --noconfirm", output, error);
}