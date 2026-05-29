using SmartFocus.Models;

namespace SmartFocus.Core.Interfaces;

public interface IHistoryTracker
{
    IReadOnlyDictionary<string, HistoryEntry> History { get; }
    void RegisterUse(string processName);
    double GetScore(string processName);
}
