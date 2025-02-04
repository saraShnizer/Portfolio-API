using GitHubService.Settings;
using Microsoft.Extensions.Caching.Memory;  // הוספת תמיכה ב-Cache
using Microsoft.Extensions.Options;
using Octokit;

public class GitHubClientService
{
    private readonly GitHubClient _client;
    private readonly IMemoryCache _cache;  // הוספת משתנה Cache

    public GitHubClientService(IOptions<GitHubSettings> options, IMemoryCache cache)
    {
        var settings = options.Value;

        if (string.IsNullOrEmpty(settings.Token))
        {
            throw new InvalidOperationException("GitHub token is not configured.");
        }

        _client = new GitHubClient(new ProductHeaderValue("PortfolioApp"))
        {
            Credentials = new Credentials(settings.Token)
        };

        _cache = cache;  // אתחול ה-Cache
    }

    public async Task<IEnumerable<dynamic>> GetPortfolioAsync(string username)
    {
        // מפתח ייחודי ל-Cache לכל משתמש
        var cacheKey = $"Portfolio_{username}";

        // בדיקה אם המידע כבר קיים ב-Cache
        if (_cache.TryGetValue(cacheKey, out IEnumerable<dynamic> cachedPortfolio))
        {
            Console.WriteLine("Fetching data from cache...");
            return cachedPortfolio;  // החזרת המידע מה-Cache
        }

        Console.WriteLine("Fetching data from GitHub API...");

        var repositories = await _client.Repository.GetAllForUser(username);
        var portfolio = repositories.Select(repo => new
        {
            Name = repo.Name,
            Language = repo.Language,
            LastCommit = repo.UpdatedAt,
            Stars = repo.StargazersCount,
            PullRequests = repo.OpenIssuesCount,
            HtmlUrl = repo.HtmlUrl
        }).ToList();

        // שמירת המידע ב-Cache עם תפוגה של 5 דקות
        _cache.Set(cacheKey, portfolio, TimeSpan.FromMinutes(5));

        return portfolio;
    }

    public async Task<IEnumerable<dynamic>> SearchRepositoriesAsync(string? repoName, string? language, string? username)
    {
        var request = new SearchRepositoriesRequest(repoName)
        {
            User = username
        };

        if (!string.IsNullOrEmpty(language))
        {
            if (Enum.TryParse<Language>(language, true, out var parsedLanguage))
            {
                request.Language = parsedLanguage;
            }
            else
            {
                throw new ArgumentException($"The provided language '{language}' is not valid.");
            }
        }

        var result = await _client.Search.SearchRepo(request);

        var repositories = result.Items.Select(repo => new
        {
            Name = repo.Name,
            FullName = repo.FullName,
            Description = repo.Description,
            Language = repo.Language,
            Stars = repo.StargazersCount,
            Forks = repo.ForksCount,
            HtmlUrl = repo.HtmlUrl
        });

        return repositories;
    }
}
