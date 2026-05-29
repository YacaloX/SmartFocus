using SmartFocus.Core.Interfaces;

namespace SmartFocus.Core;

public static class AppServices
{
    public static ISettingsManager Settings { get; } = new SettingsManager();
    public static IHistoryTracker History { get; } = new HistoryTracker();
    public static IAliasManager Aliases { get; } = new AliasManager();
    public static ISearchEngine Search { get; } = new SearchEngine(History, Aliases);
    public static IWindowManager WindowManager { get; } = new WindowManager();
}
