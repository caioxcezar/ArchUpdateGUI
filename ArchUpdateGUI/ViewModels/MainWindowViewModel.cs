using ReactiveUI;

namespace ArchUpdateGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ViewModelBase _content;
        public MainWindowViewModel()
        {
            Content = new MainViewModel();
        }
        
        public ViewModelBase Content
        {
            get => _content;
            private set => this.RaiseAndSetIfChanged(ref _content, value);
        }
    }
}