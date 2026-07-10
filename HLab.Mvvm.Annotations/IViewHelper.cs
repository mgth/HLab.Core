namespace HLab.Mvvm.Annotations;

public interface IViewHelper
{
    IMvvmContext Context { get; set; }
    object Linked { get; set; }
}
