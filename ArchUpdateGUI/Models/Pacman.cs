using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace ArchUpdateGUI.Models;

public class Pacman : IProvider
{
    public string Name => "Pacman";
    public bool RootRequired => true;

    public List<Package> Packages { get; private set; }
    public int Installed { get; private set; }
    public int Total { get; private set; }

    public void Load(bool cached)
    {
        Packages = new();
        var result = Command.Run("pacman -Sl");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        string[] list = result.Output.Split('\n');
        foreach (var package in list)
        {
            string[] listPackage = package.Split(' ');
            if (listPackage.Length > 1)
                Packages.Add(new()
                {
                    Provider = "Pacman",
                    Repository = listPackage[0],
                    Name = listPackage[1],
                    Version = listPackage[2],
                    IsInstalled = listPackage.Length == 4
                });
        }
        
        Installed = Packages.Count(p => p.IsInstalled);
        Total = Packages.Count;
    }

    public string PackageInfo(Package package)
    {
        var result = Command.Run($"pacman -Si {package.Name}");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        return result.Output;
    }

    public Task<int> Install(SecureString? pass, Package package, Action<string?> output, Action<string?> error) =>
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S pacman -Syu {package.Name} --noconfirm", output, error);

    public Task<int> Remove(SecureString? pass, Package package, Action<string?> output, Action<string?> error) =>
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S pacman -Rsu {package.Name} --noconfirm", output, error);

    public Task<int> Update(SecureString? pass, Action<string?> output, Action<string?> error) =>
        Command.Run($"echo '{pass!.SecureToString()}' | sudo -S pacman -Syu --noconfirm", output, error);

    public Command Version() => Command.Run("pacman --version");
}