namespace UpdaterGUI.Backend;

public class Package
{
    public string? Provider { get; init; }
    public string? Repository { get; init; }
    public string? Name { get; init; }
    public string? Version { get; init; }
    public string? QualifiedName { get; init; }
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