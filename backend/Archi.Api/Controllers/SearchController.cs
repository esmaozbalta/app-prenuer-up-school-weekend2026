using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Discovery;
using Archi.Api.Services.Tmdb;
using Archi.Api.Services.GoogleBooks;
using Archi.Api.Services.Rawg; // Namespace yolunu Rawg klasörüne göre ayarladık
using Microsoft.AspNetCore.Mvc;

namespace Archi.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class SearchController(
    ITmdbService tmdbService, 
    IGoogleBooksService booksService, 
    IRawgService rawgService) : ControllerBase 
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<DiscoveryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DiscoveryItemDto>>> Search(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new ErrorResponse("Query parameter 'query' is required."));
        }

    var movieTask = tmdbService.SearchAsync(query, cancellationToken);
    var bookTask = booksService.SearchBooksAsync(query, cancellationToken);
    var gameTask = rawgService.SearchGamesAsync(query, cancellationToken);

    await Task.WhenAll(movieTask, bookTask, gameTask);

    var results = new List<DiscoveryItemDto>();
    results.AddRange(movieTask.Result);
    results.AddRange(bookTask.Result);
    results.AddRange(gameTask.Result); // Artık hata vermez, çünkü hepsi DiscoveryItemDto!

    return Ok(results);
    }
}