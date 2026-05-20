using System.IO;
namespace SmartFocus.Core;

public static class AppPaths
{
    public static readonly string AppFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData
            ),
            "SmartFocus"
        );

    static AppPaths()
    {
        Directory.CreateDirectory(AppFolder);
    }
}