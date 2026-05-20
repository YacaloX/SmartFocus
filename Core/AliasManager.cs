using System.IO;
using System.Text.Json;

namespace SmartFocus.Core;

public static class AliasManager
{
    private static readonly string FilePath =
        Path.Combine(
            AppPaths.AppFolder,
            "aliases.json"
        );

    public static Dictionary<string, string> Aliases
    { get; private set; } = new();

    public static void Load()
    {
        if (!File.Exists(FilePath))
        {
            Save();
            return;
        }

        var json = File.ReadAllText(FilePath);

        Aliases =
            JsonSerializer.Deserialize<
                Dictionary<string, string>
            >(json) ?? new();
    }

    public static void Save()
    {
        var json =
            JsonSerializer.Serialize(
                Aliases,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(FilePath, json);
    }
}