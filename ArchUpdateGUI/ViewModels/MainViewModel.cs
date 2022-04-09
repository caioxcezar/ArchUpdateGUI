using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Security;
using System.Threading.Tasks;
using ArchUpdateGUI.Models;
using ArchUpdateGUI.Utils;
using DynamicData;
using MessageBox.Avalonia;
using ReactiveUI;

namespace ArchUpdateGUI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<Package> Packages { get; }
    private string _searchParam;
    private IProvider _provider;
    private List<IProvider> _providers;
    private string _info = "Loading...";
    private string _packageInfo;
    private Package _selectedPackage;
    private string _commandText;
    private bool _commandVisibility;
    public MainViewModel()
    {
        try
        {
            Packages = new();
            Providers = new Providers().List;
            this.WhenAnyValue(props => props.Provider).Subscribe(provider => ChangedProvider(provider));
            this.WhenAnyValue(props => props.SelectedPackage).Subscribe(package => ChangedPackage(package));

            IObservable<bool> canExecute = this.WhenAnyValue(props => props.CommandText, action => !string.IsNullOrWhiteSpace(action));
            
            CommandAction = ReactiveCommand.Create(() =>
            {
                switch (CommandText)
                {
                    case "Install":
                        Provider.Install(SelectedPackage);
                        break;
                    case "Remove":
                        Provider.Remove(SelectedPackage);
                        break;
                    default:
                        MessageBoxManager.GetMessageBoxStandardWindow("Ocorreu um Erro", "Opção invalida").Show();
                        break;
                }
            }, canExecute);
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandardWindow("Ocorreu um Erro", e.Message);
            msgBox.Show();
        }
    }

    private void ChangedProvider(IProvider? provider)
    {
        try
        {
            if (provider == null) return;
            Packages.Clear();
            Packages.AddRange(provider.Packages);
            Info = $"packages {provider.Installed} installed of {provider.Total}";
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandardWindow("Ocorreu um Erro", e.Message);
            msgBox.Show();
        }
    }

    public void OpenConfig()
    {
        Console.WriteLine("OpenConfig");
    }

    public void Search()
    {
        var filteredList = Provider.Packages.Where(package => package.Name.Contains(SearchParam));
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
            CommandVisibility = true;
            PackageInfo = _provider.Search(package);
            CommandText = package.IsInstalled ? "Remove" : "Install";
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager
                .GetMessageBoxStandardWindow("Ocorreu um Erro",
                    $"Não foi possível carregar as informações do pacote.\n{e.Message}");
            msgBox.Show();
        }
    }

    public void SystemUpdate()
    {
        try
        {
            Task.Run(() =>
            {
                PackageInfo = "";
                var exitCode = _provider.Update(
                    output => PackageInfo += output, 
                    error => PackageInfo += error).Result;
                PackageInfo += Command.GetErrorName(exitCode);
            });
        }
        catch (Exception e)
        {
            var msgBox = MessageBoxManager
                .GetMessageBoxStandardWindow("Ocorreu um Erro",
                    $"Não foi possível fazer a atualização.\n{e.Message}");
            msgBox.Show();
        }
    }
    public IProvider Provider
    {
        get => _provider;
        private set => this.RaiseAndSetIfChanged(ref _provider, value);
    }
    public List<IProvider> Providers
    {
        get => _providers;
        private set => this.RaiseAndSetIfChanged(ref _providers, value);
    }

    public string Info
    {
        get => _info;
        private set => this.RaiseAndSetIfChanged(ref _info, value);
    }

    public string SearchParam
    {
        get => _searchParam;
        private set => this.RaiseAndSetIfChanged(ref _searchParam, value);
    }

    public string PackageInfo
    {
        get => _packageInfo;
        private set => this.RaiseAndSetIfChanged(ref _packageInfo, value);
    }

    public Package SelectedPackage
    {
        get => _selectedPackage;
        private set => this.RaiseAndSetIfChanged(ref _selectedPackage, value);
    }
    
    public string CommandText
    {
        get => _commandText;
        private set => this.RaiseAndSetIfChanged(ref _commandText, value);
    }
    
    public bool CommandVisibility
    {
        get => _commandVisibility;
        private set => this.RaiseAndSetIfChanged(ref _commandVisibility, value);
    }

    public ReactiveCommand<Unit, Unit> CommandAction { get; }
    public Interaction<PasswordWindowViewModel, SecureString?> ShowPassword { get; } //TODO ShowPassword
}