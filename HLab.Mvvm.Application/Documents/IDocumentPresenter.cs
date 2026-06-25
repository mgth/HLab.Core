using System.Collections.ObjectModel;

namespace HLab.Mvvm.Application.Documents;

public interface IDocumentPresenter
{
    ObservableCollection<object> Documents { get; }
    ObservableCollection<object> Anchorables { get; }
    object? ActiveDocument { get; set; }
    object? Theme { get; set; }
    bool RemoveDocument(object document);
}