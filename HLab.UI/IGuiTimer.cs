namespace HLab.UI;

public interface IGuiTimer
{
    void Start();
    void Stop();
    void DoTick();
    TimeSpan Interval { get; set; }
    bool IsEnabled { get; set; }

    event EventHandler Tick;
}