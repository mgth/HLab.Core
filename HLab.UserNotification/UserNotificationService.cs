using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HLab.UserNotification;

public interface IUserNotificationService 
{
    Task AddMenuAsync(int pos, string header, string icon, Func<Task> todo);
    Task AddMenuAsync(int pos, string header, string icon, ICommand todo);

    event Action<object, object> Click;
    Task SetIconAsync(string icon, int i);

    public string ToolTipText { get; set; }
    void Show();

    /// <summary>
    /// Show or hide the tray icon. Implemented natively by the platform (Avalonia's
    /// <c>TrayIcon.IsVisible</c>), so no per-OS code is needed here; icon updates are held while
    /// hidden so a state-change refresh can't re-show it.
    /// </summary>
    bool Visible { get; set; }
}


/* TODO
    public class UserNotificationServiceBootloader : IBootloader
    {
        private readonly IUserNotificationService _userNotificationService;

        public UserNotificationServiceBootloader(IUserNotificationService service)
        {
            _userNotificationService = service;
        }

        public void Load(IBootContext bootstrapper)
        {

        }
    }
*/