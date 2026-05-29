using System.IO;
using System.Text.Json;
using SmartFocus.Core.Interfaces;
using SmartFocus.Models;

namespace SmartFocus.Core;

public class HistoryTracker : IHistoryTracker
{
    private readonly string _filePath = Path.Combine(AppPaths.AppFolder, "history.json");

    private Dictionary<string, HistoryEntry> _history = new();
    private readonly ReaderWriterLockSlim _lock = new();

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private readonly System.Timers.Timer _saveTimer = new(2000) { AutoReset = false };
    private bool _dirty;

    public HistoryTracker()
    {
        _saveTimer.Elapsed += (_, _) => FlushSave();
        Load();
    }

    public IReadOnlyDictionary<string, HistoryEntry> History => _history;

    public void Load()
    {
        _lock.EnterWriteLock();
        try
        {
            if (!File.Exists(_filePath))
            {
                _history = new();
                Save();
                return;
            }

            var json = File.ReadAllText(_filePath);
            _history = JsonSerializer.Deserialize<Dictionary<string, HistoryEntry>>(json) ?? new();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HistoryTracker.Load failed: {ex.Message}");
            _history = new();
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
            var json = JsonSerializer.Serialize(_history, _jsonOptions);
            File.WriteAllText(temp, json);
            File.Move(temp, _filePath, overwrite: true);
            _dirty = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HistoryTracker.FlushSave failed: {ex.Message}");
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

    public void RegisterUse(string processName)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_history.TryGetValue(processName, out var entry))
            {
                entry = new HistoryEntry();
                _history[processName] = entry;
            }

            entry.Frequency++;
            entry.LastUsed = DateTime.UtcNow;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        Save();
    }

    public double GetScore(string processName)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_history.TryGetValue(processName, out var entry))
                return 0;

            int maxFrequency = 0;
            foreach (var kvp in _history)
                if (kvp.Value.Frequency > maxFrequency)
                    maxFrequency = kvp.Value.Frequency;

            if (maxFrequency == 0)
                return 0;

            return (double)entry.Frequency / maxFrequency;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
