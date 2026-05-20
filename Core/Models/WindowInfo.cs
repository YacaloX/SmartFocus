namespace SmartFocus.Models;

public record WindowInfo(
    IntPtr Handle,
    string Title,
    uint ProcessId,
    string? ProcessName
);