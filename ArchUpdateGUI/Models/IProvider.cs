using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchUpdateGUI.Models;

public interface IProvider
{
    string Name { get; }
    List<Package> Packages { get; }
    int Installed { get; }
    int Total { get; }
    string Search(Package package);
    void Install(Package package);
    void Remove(Package package);
    Task<int> Update(Action<string?> output, Action<string?> error);
}