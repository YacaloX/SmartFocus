using SmartFocus.Models;

namespace SmartFocus.Core.Interfaces;

public interface ISearchEngine
{
    List<WindowInfo> Search(string query, List<WindowInfo> windows);
}
