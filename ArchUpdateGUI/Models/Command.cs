using System;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ArchUpdateGUI.Models;

public class Command
{
    public int? ExitCode { get; private set; }
    public string Output { get; private set; }
    public string Error { get; private set; }
    
    public static async Task<int> Run(string cmd, Action<string?> output, Action<string?> error)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var processInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{escapedArgs}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = new Process
        {
            StartInfo = processInfo
        };
        process.OutputDataReceived += (_, args) => output($"{args.Data}\n");
        process.ErrorDataReceived += (_, args) => error($"{args.Data}\n");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    public static Command Run(string cmd)
    {
        var command = new Command();
        var escapedArgs = cmd.Replace("\"", "\\\"");
        using var process = new Process 
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        using var output = process.StandardOutput;
        command.Output = output.ReadToEnd();
        using var error = process.StandardError;
        command.Error = error.ReadToEnd();
        process.WaitForExit();
        command.ExitCode = process.ExitCode;
        return command;
    }

    public static string ExitCodeName(int exitCode) => exitCode switch
    {
        0 => "Successful",
        1 => "General Error",
        2 => "Misuse of shell builtins",
        126 => "Command cannot be executed",
        127 => "command not found",
        128 => "Invalid argument to exit",
        130 => "Script terminated by Control-C",
        _ => Enumerable.Range(0, 255).Contains(exitCode)
            ? $"Fatal error signal \"{128 - exitCode}\""
            : "Exit status out of range",
    };
}