namespace Archi.Api.Models;

public sealed class SearchResultDto
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Film, Kitap, Dizi, Oyun
    public string Description { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}