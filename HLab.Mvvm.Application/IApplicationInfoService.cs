using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HLab.Mvvm.Application;

public interface IApplicationInfoService : INotifyPropertyChanged
{
    Version Version { get; set; }
    string Name { get; set; }
    string DataSource { get; set; }
    string Theme { get; set; }

    ObservableCollection<string> Themes { get; }
}
