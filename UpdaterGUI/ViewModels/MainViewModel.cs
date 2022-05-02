using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security;
using System.Threading.Tasks;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using UpdaterGUI.Backend;

namespace UpdaterGUI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public SmartCollection<Package> Packages { get; }
    public static ObservableCollection<Package>? ChangedPackages { get; private set; }
    private string _searchParam = "";
    private IProvider? _provider;
    private List<IProvider> _providers = new();
    private string _info = "Select a provider. ";
    private string _packageInfo = "";
    private Package? _selectedPackage;
    private bool _canExecute;
    private SecureString? _password;

    public MainViewModel(MainWindowViewModel model) : base(model)
    {
        ChangedPackages = new();
        Packages = new();
        ShowPassword = new();
        Packages = new();
        Providers = new Providers().List;
        this.WhenAnyValue(props => props.Provider).Subscribe(provider => ChangedProvider(provider, true));
        this.WhenAnyValue(props => props.SelectedPackage).Subscribe(ChangedPackage);
        this.WhenAnyValue(props => props.SearchParam).Throttle(TimeSpan.FromMilliseconds(800))
            .ObserveOn(RxApp.MainThreadScheduler).Subscribe(Search);

        IObservable<bool> canExecute = this.WhenAnyValue(props => props.CanExecute, Selector);

        CommandAction = ReactiveCommand.CreateFromTask(InnerCommandAction, canExecute);
        
        ChangedPackages.CollectionChanged += ChangedPackagesOnCollectionChanged;
    }

    private bool Selector(bool arg) => arg;

    private void ChangedPackagesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CanExecute = ChangedPackages!.Count > 0;
    }

    private async Task InnerCommandAction()
    {
        try
        {
            await UpdatePassword();
            var install = ChangedPackages!.Where(e => e.IsInstalled).ToList();
            var uninstall = ChangedPackages!.Where(e => !e.IsInstalled).ToList();
            if (install.Count > 0)
                ShowTerminal($"Installing {string.Join(' ', install.Select(p => p.Name))}", action =>
                {
                    var exitCode = Provider!.Install(_password, install,
                        action.Invoke,
                        action.Invoke).Result;
                    action.Invoke(Command.ExitCodeName(exitCode));
                    Reload();
                });
            if(uninstall.Count > 0)
                ShowTerminal($"Removing {string.Join(' ', install.Select(p => p.Name))}",action =>
                {
                    var exitCode = Provider!.Remove(_password, uninstall,
                        action.Invoke,
                        action.Invoke).Result;
                    action.Invoke(Command.ExitCodeName(exitCode));
                    Reload();
                });
            ChangedPackages!.Clear();
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", e.Message, ButtonEnum.Ok, Icon.Error).Show();
            throw;
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
            ChangedPackages!.Clear();
            await Task.Run(() => provider.Load(cached));
            Packages.Reset(provider.Packages);
            Info = $"packages {provider.Installed} installed of {provider.Total}";
            Search(SearchParam);
        }
        catch (Exception e)
        {
            await MessageBoxManager
                .GetMessageBoxStandardWindow("A error has occurred", e.Message, ButtonEnum.Ok, Icon.Error).Show();
        }
        finally
        {
            ShowLoading(false);
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
            await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", e.Message, ButtonEnum.Ok, Icon.Error).Show();
        }
    }

    public void Search(string param)
    {
        if (Provider == null) return;
        var filteredList = param.Trim() == ""
            ? Provider.Packages
            : Provider.Packages.Where(package =>
                package.Name != null && package.Name.ToLower().Contains(param.ToLower()));
        Packages.Reset(filteredList);
    }
    
    public void ChangedPackage(Package? package)
    {
        try
        {
            if (package == null) return;
            PackageInfo = Provider!.PackageInfo(package);
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager
                .GetMessageBoxStandardWindow("A error has occurred",
                    $"Was not possible to archive package information.\n{e.Message}", ButtonEnum.Ok, Icon.Error);
            msgBox.Show();
        }
    }

    
    public async void Update()
    {
        try
        {
            await UpdatePassword();
            ShowTerminal($"Updating {Provider!.Name}", action =>
            {
                var exitCode = Provider.Update(_password, 
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
                    $"Was not possible to make update.\n{e.Message}", ButtonEnum.Ok, Icon.Error)
                .Show();
        }
    }


    public async void UpdateAll()
    {
        if (Providers.FirstOrDefault(p => p.RootRequired) != null && _password == null)
        {
            _password = await ShowPassword.Handle(new PasswordViewModel());
            if (_password == null)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", "Invalid Password. ",
                    ButtonEnum.Ok, Icon.Warning).Show();
                return;
            }
        }
        ShowTerminal("Updating",action =>
        {
            foreach (var provider in Providers)
            {
                Command.Run("sudo -k");
                action.Invoke($"►▻►{provider.Name}\n");
                var exitCode = provider.Update(_password,
                    action.Invoke,
                    action.Invoke).Result;
                action.Invoke($"{Command.ExitCodeName(exitCode)}\n");
            }
        });
    }

    private async Task UpdatePassword()
    {
        if (Provider is {RootRequired: false} || _password != null) return;
        _password = await ShowPassword.Handle(new PasswordViewModel());
        if (_password == null)
            await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", "Invalid Password. ",
                ButtonEnum.Ok, Icon.Warning).Show();
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
    
    public bool CanExecute
    {
        get => _canExecute;
        set => this.RaiseAndSetIfChanged(ref _canExecute, value);
    }

    public ReactiveCommand<Unit, Unit> CommandAction { get; }
    public Interaction<PasswordViewModel, SecureString?> ShowPassword { get; }
}