using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace ArchUpdateGUI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        ViewModelBase _current;
        private List<ViewModelBase> _stack;
        
        public MainWindowViewModel()
        {
            _stack = new();
            Navigate(typeof(MainViewModel));
        }
        
        public void Navigate(Type newPage, params object?[]? args)
        {
            var innerArgs = args == null ? new object?[] {this} : args.Prepend(this).ToArray();
            var obj = (ViewModelBase)Activator.CreateInstance(newPage, innerArgs)!;
            _stack.Add(obj);
            Current = _stack.Last();
        }

        public void GoBack()
        {
            if (_stack.Count < 1) return;
            _stack.Remove(_stack.Last());
            Current = _stack.Last();
        }
        
        public ViewModelBase Current
        {
            get => _current;
            private set => this.RaiseAndSetIfChanged(ref _current, value);
        }
    }
}