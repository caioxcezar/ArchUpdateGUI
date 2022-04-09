using System;

namespace ArchUpdateGUI.Models;

public class CommandException : Exception
{
    public CommandException(string message) : base(message) { }
}