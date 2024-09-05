using System.Threading.Tasks;
using HLab.Core.Annotations;

namespace HLab.Mvvm.Annotations;

public interface IDialogService : IService
{
    void ShowMessageOk(string text, string caption, string icon);
    bool ShowMessageOkCancel(string text, string caption, string icon);
    bool ShowMessageYesNo(string text, string caption, string icon);
    bool? ShowMessageYesNoCancel(string text, string caption, string icon);
    Task ShowMessageOkAsync(string text, string caption, string icon);
    Task<bool> ShowMessageOkCancelAsync(string text, string caption, string icon);
    Task<bool> ShowMessageYesNoAsync(string text, string caption, string icon);
    Task<bool?> ShowMessageYesNoCancelAsync(string text, string caption, string icon);
}
