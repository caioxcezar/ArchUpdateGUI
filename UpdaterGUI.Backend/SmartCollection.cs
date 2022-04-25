using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace UpdaterGUI.Backend;

public class SmartCollection<T> : ObservableCollection<T> 
{
    public void AddRange(IEnumerable<T> items)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        foreach (var item in items) Items.Add(item);
        
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));        
    }

    public void Reset(IEnumerable<T> items)
    {
        Items.Clear();
        AddRange(items);
    }
}