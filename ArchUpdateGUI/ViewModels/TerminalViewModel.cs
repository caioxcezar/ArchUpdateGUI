using System;
using System.Threading.Tasks;
using ReactiveUI;

namespace ArchUpdateGUI.ViewModels;

public class TerminalViewModel : ViewModelBase
{
    public string _terminalText;
    public TerminalViewModel(MainWindowViewModel main, Action<Action<string?>> action) : base(main)
    {
        TerminalText = "";
         Task.Run(() => action.Invoke(str => TerminalText += str));
    }

    public string TerminalText
    {
        get => _terminalText;
        set => this.RaiseAndSetIfChanged(ref _terminalText, value);
    }
    public void Back() => GoBack();
}