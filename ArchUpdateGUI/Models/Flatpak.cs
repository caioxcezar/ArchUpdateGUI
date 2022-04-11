using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DynamicData.Kernel;

namespace ArchUpdateGUI.Models;

public class Flatpak : IProvider
{
    public string Name => "Flatpak";
    public bool RootRequired => false;
    public List<Package> Packages { get; private set; }
    public int Installed { get; private set; }
    public int Total { get; private set; }

    public void Load()
    {
        Packages = new();
        var result = Command.Run("flatpak remotes");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        foreach (var remote in result.Output.Split('\n'))
        {
            var listRemote = remote.Split('\t');
            if(remote.Length < 1 || listRemote[0] == "Name") continue;
            var subResult = Command.Run($"flatpak remote-ls {listRemote[0]}");
            if (subResult.ExitCode != 0) throw new CommandException(subResult.Error);
            foreach (var package in subResult.Output.Split('\n'))
            {
                var listPackage = package.Split('\t');
                if (listPackage.Length < 2) continue;
                Packages.Add(new Package
                {
                    Provider = "Flatpak",
                    Repository = listRemote[0],
                    Name = listPackage[0],
                    QualifiedName = listPackage[1],
                    Version = listPackage[2]
                });
            }
        }
        result = Command.Run("flatpak list");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        var installedPackage = result.Output.Split('\n').Where(e => e.Split('\t').Length > 1).AsList();
        Parallel.ForEach(Packages, package =>
        {
            package.IsInstalled = installedPackage.FirstOrOptional(e => e.Contains(package.Name)).HasValue;
        });
        Total = Packages.Count;
        Installed = installedPackage.Count;
    }

    public string PackageInfo(Package package)
    {
        var result = Command.Run($"flatpak search {package.QualifiedName}");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        var fields = result.Output.Split('\t');
        return fields.Length > 1
            ? $"Name: {fields[0]}\nDescription: {fields[1]}\nApplication ID: {fields[2]}\nVersion: {fields[3]}\nBranch: {fields[4]}\nRemotes: {fields[5]}\n"
            : result.Output;
    }

    public Task<int> Install(SecureString? pass, Package package, Action<string?> output, Action<string?> error) => 
        Command.Run($"flatpak install {package.QualifiedName} -y", output, error);

    public Task<int> Remove(SecureString? pass, Package package, Action<string?> output, Action<string?> error) => 
        Command.Run($"flatpak remove {package.QualifiedName} -y", output, error);

    public Command Version() => Command.Run($"flatpak --version");

    public Task<int> Update(SecureString? pass, Action<string?> output, Action<string?> error) => 
        Command.Run("flatpak update -y", output, error);
}