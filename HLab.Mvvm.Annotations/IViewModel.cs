namespace HLab.Mvvm.Annotations;

public interface IMvvmContextProvider
{
    void ConfigureMvvmContext(IMvvmContext ctx) { }
}

public interface IViewModel
{
    IMvvmContext? MvvmContext { get; set; }

    object? Model { get; set; }
}

public interface IViewModel<T> : IViewModel
    where T : class?
{
    new T? Model { get; set; }
}
