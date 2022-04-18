namespace ArchUpdateGUI.Models;

public class Package
{
    public string? Provider { get; set; }
    public string? Repository { get; set; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? QualifiedName { get; set; }
    public bool IsInstalled { get; set; }

    public Package Clone() => new()
    {
        Name = Name,
        Provider = Provider,
        Repository = Repository,
        Version = Version,
        IsInstalled = IsInstalled,
        QualifiedName = QualifiedName
    };
}