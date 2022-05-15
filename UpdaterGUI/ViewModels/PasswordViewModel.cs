using System.Reactive;
using System.Security;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using UpdaterGUI.Backend;

namespace UpdaterGUI.ViewModels;

public class PasswordViewModel : ReactiveObject
{
    private string _password;
    public PasswordViewModel()
    {
        _password = "";
        Ok = ReactiveCommand.Create(() =>
        {
            var result = Command.Run($"echo '{Password}' | sudo -S su");
            Command.Run("sudo -k");
            if (result.ExitCode != 0)
            {
                MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", "Invalid Password. ",
                    ButtonEnum.Ok, Icon.Warning).Show();
                return null;
            }
            var pass = new SecureString();
            foreach (var c in Password.ToCharArray())
                pass.AppendChar(c);
            pass.MakeReadOnly();
            return pass;
        });
    }
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }
    public ReactiveCommand<Unit, SecureString?> Ok { get; }
}