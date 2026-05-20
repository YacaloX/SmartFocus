using SmartFocus.Models;

namespace SmartFocus.Core;

public class SearchEngine
{
    public List<WindowInfo> Search(
        string query,
        List<WindowInfo> windows)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return windows.Take(5).ToList();
        }

        // Alias
        if (AliasManager.Aliases.TryGetValue(
            query,
            out var expansion))
        {
            query = expansion;
        }

        var normalizedQuery =
            query.Trim().ToLowerInvariant();

        var scored = windows
            .Select(w =>
            {
                string appName =
                    w.ProcessName ?? w.Title;

                string lowerApp =
                    appName.ToLowerInvariant();

                string lowerTitle =
                    w.Title.ToLowerInvariant();

                double score = 0;

                // Exacta
                if (lowerApp == normalizedQuery)
                    score += 100;

                // Prefijo app
                else if (lowerApp.StartsWith(normalizedQuery))
                    score += 80;

                // Prefijo título
                else if (lowerTitle.StartsWith(normalizedQuery))
                    score += 70;

                // Contains app
                else if (lowerApp.Contains(normalizedQuery))
                    score += 60;

                // Contains title
                else if (lowerTitle.Contains(normalizedQuery))
                    score += 50;

                else
                {
                    // Fuzzy
                    int maxLen = Math.Max(
                        lowerApp.Length,
                        normalizedQuery.Length
                    );

                    int dist = LevenshteinDistance(
                        lowerApp,
                        normalizedQuery
                    );

                    double ratio =
                        1.0 - (double)dist / maxLen;

                    if (ratio > 0.6)
                    {
                        score += ratio * 40;
                    }
                }

                // Historial
                score +=
                    HistoryTracker.GetScore(
                        w.ProcessName ?? ""
                    ) * 10;

                return new
                {
                    Window = w,
                    Score = score
                };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Window.Title)
            .Take(8)
            .Select(x => x.Window)
            .ToList();

        return scored;
    }

    private int LevenshteinDistance(
        string a,
        string b)
    {
        int[,] dp =
            new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
            dp[i, 0] = i;

        for (int j = 0; j <= b.Length; j++)
            dp[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost =
                    a[i - 1] == b[j - 1] ? 0 : 1;

                dp[i, j] = Math.Min(
                    Math.Min(
                        dp[i - 1, j] + 1,
                        dp[i, j - 1] + 1
                    ),
                    dp[i - 1, j - 1] + cost
                );
            }
        }

        return dp[a.Length, b.Length];
    }
}