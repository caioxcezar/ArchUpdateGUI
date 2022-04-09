using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchUpdateGUI.Utils;
using DynamicData.Kernel;

namespace ArchUpdateGUI.Models;

public class ProtonUp : IProvider
{
    public string Name => "ProtonUp";
    public List<Package> Packages { get; }
    public int Installed { get; }
    public int Total { get; }

    public ProtonUp()
    {
        var result = Command.Run("protonup --releases");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        Packages = result.Output.Split('\n').Select(version => new Package
        {
            Provider = Name,
            Version = version,
            Name = "Proton GE"
        }).ToList();
        result = Command.Run("protonup -l");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
        var installed = result.Output.Split("\n").ToList();
        Parallel.ForEach(Packages, package =>
        {
            package.IsInstalled = installed.FirstOrOptional(i => i.Contains(package.Version)).HasValue;
        });
    }
    public string Search(Package package) => Packages.First(p => p.Version == package.Version).Version;

    public void Install(Package package)
    {
        var result = Command.Run($"protonup -t {package.Version}");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
    }

    public void Remove(Package package)
    {
        var result = Command.Run($"protonup -r {package.Version}");
        if (result.ExitCode != 0) throw new CommandException(result.Error);
    }

    public Task<int> Update(Action<string?> output, Action<string?> error) =>
        Command.Run("protonup -y", output, error);
}