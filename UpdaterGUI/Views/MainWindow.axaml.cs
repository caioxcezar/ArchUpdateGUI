using System.Security;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using ReactiveUI;
using UpdaterGUI.ViewModels;

namespace UpdaterGUI.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                var x = (MainViewModel)ViewModel!.Current;
                d(x.ShowPassword.RegisterHandler(DoShowDialogAsync));
            });
        }
        
        private async Task DoShowDialogAsync(InteractionContext<PasswordViewModel, SecureString?> interaction)
        {
            var dialog = new PasswordWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<SecureString?>(this);
            interaction.SetOutput(result);
        }
    }
}