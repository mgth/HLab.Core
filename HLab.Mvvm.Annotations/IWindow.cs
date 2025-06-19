namespace HLab.Mvvm.Annotations;

public interface IWindow
{
   void SetOwner(IView owner);
   bool? ShowDialog();
   void Show();
}