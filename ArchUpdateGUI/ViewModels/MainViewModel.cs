using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security;
using System.Threading.Tasks;
using ArchUpdateGUI.Backend;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace ArchUpdateGUI.ViewModels;

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
    public MainViewModel(MainWindowViewModel model) : base(model)
    {
        ChangedPackages = new();
        Packages = new();
        ShowPassword = new();
        Packages = new();
        Providers = new Providers().List;
        this.WhenAnyValue(props => props.Provider).Subscribe(provider => ChangedProvider(provider, true));
        this.WhenAnyValue(props => props.SelectedPackage).Subscribe(ChangedPackage);
        this.WhenAnyValue(props => props.SearchParam).Where(param => !string.IsNullOrWhiteSpace(param))
            .Throttle(TimeSpan.FromMilliseconds(400)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => Search());

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
        SecureString? pass = null;
        if (Provider!.RootRequired)
        {
            pass = await ShowPassword.Handle(new PasswordViewModel());
            if (pass == null)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("A error has occurred", "Invalid Password. ", ButtonEnum.Ok, Icon.Warning).Show();
                return;
            }
        }
        try
        {
            var install = ChangedPackages!.Where(e => e.IsInstalled).ToList();
            var uninstall = ChangedPackages!.Where(e => !e.IsInstalled).ToList();
            if (install.Count > 0)
                ShowTerminal($"Instaling {string.Join(' ', install.Select(p => p.Name))}", action =>
                {
                    var exitCode = Provider.Install(pass, install,
                        action.Invoke,
                        action.Invoke).Result;
                    action.Invoke(Command.ExitCodeName(exitCode));
                    Reload();
                });
            if(uninstall.Count > 0)
                ShowTerminal($"Removing {string.Join(' ', install.Select(p => p.Name))}",action =>
                {
                    var exitCode = Provider.Remove(pass, uninstall,
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
            if (Provider == null) return;
            var pass = Provider!.RootRequired ? await ShowPassword.Handle(new PasswordViewModel()) : null;
            if (pass == null && Provider.RootRequired)
            {
                await MessageBoxManager
                    .GetMessageBoxStandardWindow("Wrong password",
                        $"Please provide the correct password. ", ButtonEnum.Ok, Icon.Warning)
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
                    $"Was not possible to make update.\n{e.Message}", ButtonEnum.Ok, Icon.Error)
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
                        $"Please provide the correct password. ", ButtonEnum.Ok, Icon.Warning)
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
    
    public bool CanExecute
    {
        get => _canExecute;
        set => this.RaiseAndSetIfChanged(ref _canExecute, value);
    }
    
    public ReactiveCommand<Unit, Unit> CommandAction { get; }
    public Interaction<PasswordViewModel, SecureString?> ShowPassword { get; }
}