using SmartFocus.Models;

namespace SmartFocus.Core.Interfaces;

public interface IWindowManager
{
    List<WindowInfo> GetAllWindows();
    Task FocusWindowAsync(IntPtr hwndPtr);
}
