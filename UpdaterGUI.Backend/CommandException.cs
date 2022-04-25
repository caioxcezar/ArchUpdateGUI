namespace UpdaterGUI.Backend;

public class CommandException : Exception
{
    public CommandException(string message) : base(message) { }
}