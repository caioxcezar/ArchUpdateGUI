using System;
using System.Threading.Tasks;
using ReactiveUI;

namespace ArchUpdateGUI.ViewModels;

public class TerminalViewModel : ViewModelBase
{
    private string _terminalText = "";
    private string _title = "";

    public TerminalViewModel(MainWindowViewModel main, string title, Action<Action<string?>> action) : base(main)
    {
        Title = title;
        Task.Run(() => action.Invoke(str => TerminalText += str));
    }

    public string TerminalText
    {
        get => _terminalText;
        set => this.RaiseAndSetIfChanged(ref _terminalText, value);
    }
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    public void Back() => GoBack();
}