namespace SmartFocus.Core.Interfaces;

public interface IAliasManager
{
    IReadOnlyDictionary<string, string> Aliases { get; }
}
