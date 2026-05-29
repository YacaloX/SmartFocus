using System.IO;
using System.Text.Json;
using SmartFocus.Core.Interfaces;

namespace SmartFocus.Core;

public class AliasManager : IAliasManager
{
    private readonly string _filePath = Path.Combine(AppPaths.AppFolder, "aliases.json");

    private Dictionary<string, string> _aliases = new();
    private readonly ReaderWriterLockSlim _lock = new();

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private readonly System.Timers.Timer _saveTimer = new(2000) { AutoReset = false };
    private bool _dirty;

    public AliasManager()
    {
        _saveTimer.Elapsed += (_, _) => FlushSave();
        Load();
    }

    public IReadOnlyDictionary<string, string> Aliases => _aliases;

    public void Load()
    {
        _lock.EnterWriteLock();
        try
        {
            if (!File.Exists(_filePath))
            {
                _aliases = new();
                Save();
                return;
            }

            var json = File.ReadAllText(_filePath);
            _aliases = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AliasManager.Load failed: {ex.Message}");
            _aliases = new();
            Save();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private void FlushSave()
    {
        _lock.EnterReadLock();
        try
        {
            if (!_dirty) return;
            string dir = Path.GetDirectoryName(_filePath)!;
            Directory.CreateDirectory(dir);
            string temp = Path.GetTempFileName();
            var json = JsonSerializer.Serialize(_aliases, _jsonOptions);
            File.WriteAllText(temp, json);
            File.Move(temp, _filePath, overwrite: true);
            _dirty = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AliasManager.FlushSave failed: {ex.Message}");
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Save()
    {
        _lock.EnterReadLock();
        _dirty = true;
        _lock.ExitReadLock();
        _saveTimer.Stop();
        _saveTimer.Start();
    }
}
