using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security;
using System.Threading.Tasks;
using ArchUpdateGUI.Models;
using AvaloniaEdit.Utils;
using DynamicData;
using MessageBox.Avalonia;
using ReactiveUI;

namespace ArchUpdateGUI.ViewModels;

public class MainViewModel : ViewModelBase
{
    
    public ObservableCollection<Package> Packages { get; private set; }
    private string _searchParam = "";
    private IProvider? _provider;
    private List<IProvider> _providers = new();
    private string _info = "Select a provider. ";
    private string _packageInfo = "";
    private Package? _selectedPackage;
    private string _commandText = "";
    private bool _commandVisibility;
    public MainViewModel(MainWindowViewModel model) : base(model)
    {
        Packages = new();
        ShowPassword = new();
        Packages = new();
        Providers = new Providers().List;
        this.WhenAnyValue(props => props.Provider).Subscribe(provider => ChangedProvider(provider, true));
        this.WhenAnyValue(props => props.SelectedPackage).Subscribe(ChangedPackage);
        this.WhenAnyValue(props => props.SearchParam).Where(param => !string.IsNullOrWhiteSpace(param))
            .Throttle(TimeSpan.FromMilliseconds(400)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => Search());

        IObservable<bool> canExecute = this.WhenAnyValue(props => props.CommandText, action => !string.IsNullOrWhiteSpace(action));

        CommandAction = ReactiveCommand.CreateFromTask(InnerCommandAction, canExecute);
    }

    private async Task InnerCommandAction()
    {
        var pass = Provider!.RootRequired ? await ShowPassword.Handle(new PasswordViewModel()) : null;
        switch (CommandText)
        {
            case "Install":
                ShowTerminal($"Instaling {SelectedPackage!.Name}", action =>
                {
                    var exitCode = Provider.Install(pass, SelectedPackage,
                        action.Invoke,
                        action.Invoke).Result;
                    action.Invoke(Command.ExitCodeName(exitCode));
                    Reload();
                });
                break;
            case "Remove":
                ShowTerminal($"Removing {SelectedPackage!.Name}",action =>
                {
                    var exitCode = Provider.Remove(pass, SelectedPackage,
                        action.Invoke,
                        action.Invoke).Result;
                    action.Invoke(Command.ExitCodeName(exitCode));
                    Reload();
                });
                break;
            default:
                await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", "Invalid Option. ").Show();
                break;
        }
    }

    private void Reload() => ChangedProvider(Provider, false);

    private void ShowTerminal(string title, Action<Action<string?>> action) => Navigate(typeof(TerminalViewModel), title, action);

    private async void ChangedProvider(IProvider? provider, bool cached)
    {
        try
        {
            if (provider == null) return;
            ShowLoading(true);
            await Task.Run(() => provider.Load(cached));
            Packages.Clear();
            Packages.AddRange(provider.Packages);
            Info = $"packages {provider.Installed} installed of {provider.Total}";
            ShowLoading(false);
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", e.Message).Show();
        }
    }
    
    public async Task OpenConfig()
    {
        try
        {
            Navigate(typeof(ConfigViewModel));
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", e.Message).Show();
        }
    }
    
    public void Search()
    {
        if (Provider == null) return;
        var filteredList = Provider.Packages.Where(package =>
            package.Name != null && package.Name.ToLower().Contains(SearchParam.ToLower()));
        Packages.Clear();
        Packages.AddRange(filteredList);
    }
    
    public void ChangedPackage(Package? package)
    {
        try
        {
            if (package == null)
            {
                CommandVisibility = false;
                return;
            }
            PackageInfo = Provider!.PackageInfo(package);
            CommandText = package.IsInstalled ? "Remove" : "Install";
            CommandVisibility = true;
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager
                .GetMessageBoxStandardWindow("A error has occurred",
                    $"Was not possible to archive package information.\n{e.Message}");
            msgBox.Show();
        }
    }

    
    public async void Update()
    {
        try
        {
            if (Provider == null) return;
            var pass = Provider!.RootRequired ? await ShowPassword.Handle(new PasswordViewModel()) : null;
            if (pass == null && Provider.RootRequired)
            {
                await MessageBoxManager
                    .GetMessageBoxStandardWindow("Wrong password",
                        $"Please provide the correct password. ")
                    .Show();
                return;
            }
            ShowTerminal($"Updating {Provider.Name}", action =>
            {
                var exitCode = Provider.Update(pass, 
                    action.Invoke, 
                    action.Invoke).Result;
               action.Invoke(Command.ExitCodeName(exitCode));
               Reload();
            });
        }
        catch (Exception e)
        {
            await MessageBoxManager
                .GetMessageBoxStandardWindow("A error has occurred",
                    $"Was not possible to make update.\n{e.Message}")
                .Show();
        }
    }


    public async void UpdateAll()
    {
        SecureString? pass = null;
        if (Providers.FirstOrDefault(p => p.RootRequired) != null)
        {
            pass = await ShowPassword.Handle(new PasswordViewModel());
            if (pass == null)
            {
                await MessageBoxManager
                    .GetMessageBoxStandardWindow("Wrong password",
                        $"Please provide the correct password. ")
                    .Show();
                return;
            }
        }
        ShowTerminal("Updating",action =>
        {
            foreach (var provider in Providers)
            {
                Command.Run("sudo -k");
                action.Invoke($"►▻►{provider.Name}\n");
                var exitCode = provider.Update(pass,
                    action.Invoke,
                    action.Invoke).Result;
                action.Invoke($"{Command.ExitCodeName(exitCode)}\n");
                //Reload(provider);
            }
        });
    }
    
    public IProvider? Provider
    {
        get => _provider;
        set => this.RaiseAndSetIfChanged(ref _provider, value);
    }
    
    public List<IProvider> Providers
    {
        get => _providers;
        set => this.RaiseAndSetIfChanged(ref _providers, value);
    }
    
    public string Info
    {
        get => _info;
        private set => this.RaiseAndSetIfChanged(ref _info, value);
    }
    
    public string SearchParam
    {
        get => _searchParam;
        set => this.RaiseAndSetIfChanged(ref _searchParam, value);
    }
    
    public string PackageInfo
    {
        get => _packageInfo;
        set => this.RaiseAndSetIfChanged(ref _packageInfo, value);
    }
    
    public Package? SelectedPackage
    {
        get => _selectedPackage;
        set => this.RaiseAndSetIfChanged(ref _selectedPackage, value);
    }
    
    public string CommandText
    {
        get => _commandText;
        set => this.RaiseAndSetIfChanged(ref _commandText, value);
    }
    
    public bool CommandVisibility
    {
        get => _commandVisibility;
        set => this.RaiseAndSetIfChanged(ref _commandVisibility, value);
    }
    
    public ReactiveCommand<Unit, Unit> CommandAction { get; }
    public Interaction<PasswordViewModel, SecureString?> ShowPassword { get; }
}