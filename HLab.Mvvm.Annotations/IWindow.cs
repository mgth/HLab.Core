namespace HLab.Mvvm.Annotations;

public interface IWindow
{
   object DataContext { get; set; }
   IView? View { get; set; }
   void SetOwner(IView owner);
   bool? ShowDialog();
   void Show();
}