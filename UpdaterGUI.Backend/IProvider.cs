using System.Security;

namespace UpdaterGUI.Backend;

public interface IProvider
{
    string Name { get; }
    List<Package> Packages { get; }
    int Installed { get; }
    int Total { get; }
    bool RootRequired { get; }
    void Load(bool cached);
    Command Version();
    string PackageInfo(Package package);
    Task<int> Install(SecureString? pass, IList<Package> package, Action<string?> output, Action<string?> error);
    Task<int> Remove(SecureString? pass, IList<Package> package, Action<string?> output, Action<string?> error);
    Task<int> Update(SecureString? pass, Action<string?> output, Action<string?> error);
}