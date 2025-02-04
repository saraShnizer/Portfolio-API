using Microsoft.AspNetCore.Mvc;
using GitHubService;

[ApiController]
[Route("api/[controller]")]
public class RepositoriesController : ControllerBase
{
    private readonly GitHubClientService _gitHubClientService;

    public RepositoriesController(GitHubClientService gitHubClientService)
    {
        _gitHubClientService = gitHubClientService;
    }


    [HttpGet("portfolio/{username}")]
    public async Task<IActionResult> GetPortfolio(string username)
    {
        var portfolio = await _gitHubClientService.GetPortfolioAsync(username);
        return Ok(portfolio);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRepositories([FromQuery] string? repoName, [FromQuery] string? language, [FromQuery] string? username)
    {
        try
        {
            var repositories = await _gitHubClientService.SearchRepositoriesAsync(repoName, language, username);
            return Ok(repositories);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { Error = "Configuration error.", Details = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
        }
    }
}