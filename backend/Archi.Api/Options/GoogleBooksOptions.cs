namespace Archi.Api.Options;

public sealed class GoogleBooksOptions
{
    public const string SectionName = "GoogleBooks";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://www.googleapis.com/books/v1";
}
