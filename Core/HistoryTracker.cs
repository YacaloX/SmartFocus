using System.IO;
using System.Text.Json;
using SmartFocus.Models;

namespace SmartFocus.Core;

public static class HistoryTracker
{
    private static readonly string FilePath =
        Path.Combine(
            AppPaths.AppFolder,
            "history.json"
        );

    public static Dictionary<string, HistoryEntry> History
    { get; private set; } = new();

    public static void Load()
    {
        if (!File.Exists(FilePath))
        {
            Save();
            return;
        }

        var json = File.ReadAllText(FilePath);

        History =
            JsonSerializer.Deserialize<
                Dictionary<string, HistoryEntry>
            >(json) ?? new();
    }

    public static void Save()
    {
        var json =
            JsonSerializer.Serialize(
                History,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(FilePath, json);
    }

    public static void RegisterUse(string processName)
    {
        if (!History.TryGetValue(
            processName,
            out var entry))
        {
            entry = new HistoryEntry();

            History[processName] = entry;
        }

        entry.Frequency++;
        entry.LastUsed = DateTime.Now;

        Save();
    }

    public static double GetScore(string processName)
    {
        if (!History.TryGetValue(
            processName,
            out var entry))
        {
            return 0;
        }

        int maxFrequency =
            History.Values.Max(x => x.Frequency);

        if (maxFrequency == 0)
            return 0;

        double normalized =
            (double)entry.Frequency / maxFrequency;

        return normalized;
    }
}