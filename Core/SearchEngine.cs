using SmartFocus.Core.Interfaces;
using SmartFocus.Models;

namespace SmartFocus.Core;

public class SearchEngine : ISearchEngine
{
    private const int MaxResults = 8;
    private readonly IHistoryTracker _history;
    private readonly IAliasManager _aliases;

    public SearchEngine(IHistoryTracker history, IAliasManager aliases)
    {
        _history = history;
        _aliases = aliases;
    }

    public List<WindowInfo> Search(string query, List<WindowInfo> windows)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return windows.Take(MaxResults).ToList();
        }

        if (_aliases.Aliases.TryGetValue(query, out var expansion))
        {
            query = expansion;
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();

        var scored = windows
            .Select(w =>
            {
                string appName = w.ProcessName ?? w.Title;
                string lowerApp = appName.ToLowerInvariant();
                string lowerTitle = w.Title.ToLowerInvariant();

                double score = 0;

                if (lowerApp == normalizedQuery)
                    score += 100;
                else if (lowerApp.StartsWith(normalizedQuery))
                    score += 80;
                else if (lowerTitle.StartsWith(normalizedQuery))
                    score += 70;
                else if (lowerApp.Contains(normalizedQuery))
                    score += 60;
                else if (lowerTitle.Contains(normalizedQuery))
                    score += 50;
                else
                {
                    int dist = LevenshteinDistance(lowerApp, normalizedQuery);
                    double ratio = 1.0 - (double)dist / Math.Max(lowerApp.Length, normalizedQuery.Length);
                    if (ratio > 0.6)
                        score += ratio * 40;
                }

                score += _history.GetScore(w.ProcessName ?? "") * 10;

                return (Window: w, Score: score);
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Window.Title)
            .Take(MaxResults)
            .Select(x => x.Window)
            .ToList();

        return scored;
    }

    private int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
        if (string.IsNullOrEmpty(b)) return a.Length;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];

        for (int j = 0; j <= b.Length; j++)
            prev[j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }

        return prev[b.Length];
    }
}
