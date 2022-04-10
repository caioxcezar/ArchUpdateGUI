using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

namespace ArchUpdateGUI.Models;

public interface IProvider
{
    string Name { get; }
    List<Package> Packages { get; }
    int Installed { get; }
    int Total { get; }
    bool RootRequired { get; }
    void Load();
    string PackageInfo(Package package);
    Task<int> Install(SecureString? pass, Package package, Action<string?> output, Action<string?> error);
    Task<int> Remove(SecureString? pass, Package package, Action<string?> output, Action<string?> error);
    Task<int> Update(SecureString? pass, Action<string?> output, Action<string?> error);
}