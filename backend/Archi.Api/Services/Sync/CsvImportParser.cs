using System.Globalization;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Search;
using Archi.Api.Models;

namespace Archi.Api.Services.Sync;

public sealed class CsvImportParser
{
    public IReadOnlyList<ImportArchiveRow> Parse(byte[] csvBytes, int maxRows)
    {
        using var stream = new MemoryStream(csvBytes);
        using var reader = new StreamReader(stream);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return [];
        }

        var headers = ParseLine(headerLine)
            .Select(NormalizeHeader)
            .ToList();

        var format = DetectFormat(headers);
        var rows = new List<ImportArchiveRow>();

        string? line;
        while ((line = reader.ReadLine()) is not null && rows.Count < maxRows)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseLine(line);
            if (values.Count == 0)
            {
                continue;
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count && i < values.Count; i++)
            {
                map[headers[i]] = values[i].Trim();
            }

            var parsed = format switch
            {
                CsvImportFormat.Goodreads => ParseGoodreadsRow(map),
                CsvImportFormat.Letterboxd => ParseLetterboxdRow(map),
                _ => null
            };

            if (parsed is not null)
            {
                rows.Add(parsed);
            }
        }

        return rows;
    }

    private static CsvImportFormat DetectFormat(IReadOnlyList<string> headers)
    {
        var set = headers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (set.Contains("letterboxduri") || set.Contains("letterboxd uri") ||
            (set.Contains("name") && set.Contains("year")))
        {
            return CsvImportFormat.Letterboxd;
        }

        if (set.Contains("bookid") || set.Contains("book id") || set.Contains("bookshelves") ||
            set.Contains("isbn") || set.Contains("title"))
        {
            return CsvImportFormat.Goodreads;
        }

        return CsvImportFormat.Unknown;
    }

    private static ImportArchiveRow? ParseGoodreadsRow(IReadOnlyDictionary<string, string> map)
    {
        var title = Get(map, "title");
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var author = Get(map, "author");
        var bookId = Get(map, "bookid", "book id", "isbn13", "isbn");
        var externalId = string.IsNullOrWhiteSpace(bookId)
            ? $"gr-{title.GetHashCode(StringComparison.Ordinal):X}"
            : bookId;

        var year = ParseYear(Get(map, "yearpublished", "year published", "original publication year"));
        var shelves = Get(map, "bookshelves", "exclusive shelf");
        var status = MapShelfStatus(shelves);

        return new ImportArchiveRow(
            externalId,
            MediaCategories.Book,
            title,
            new ArchiveMetadata { Author = author, Year = year },
            status);
    }

    private static ImportArchiveRow? ParseLetterboxdRow(IReadOnlyDictionary<string, string> map)
    {
        var title = Get(map, "name", "title", "film");
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var uri = Get(map, "letterboxduri", "letterboxd uri", "uri");
        var year = ParseYear(Get(map, "year"));
        var externalId = string.IsNullOrWhiteSpace(uri)
            ? $"lb-{title}-{year}"
            : uri;

        return new ImportArchiveRow(
            externalId,
            MediaCategories.Movie,
            title,
            new ArchiveMetadata { Year = year },
            ArchiveItemStatus.Done);
    }

    private static ArchiveItemStatus MapShelfStatus(string? shelf)
    {
        if (string.IsNullOrWhiteSpace(shelf))
        {
            return ArchiveItemStatus.Wishlist;
        }

        var normalized = shelf.Trim().ToLowerInvariant();
        return normalized switch
        {
            "currently-reading" or "currently-reading," => ArchiveItemStatus.InProgress,
            "read" => ArchiveItemStatus.Done,
            _ => ArchiveItemStatus.Wishlist
        };
    }

    private static int? ParseYear(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) ? year : null;

    private static string? Get(IReadOnlyDictionary<string, string> map, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (map.TryGetValue(NormalizeHeader(key), out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string NormalizeHeader(string header) =>
        header.Trim().ToLowerInvariant().Replace(" ", string.Empty, StringComparison.Ordinal);

    private static List<string> ParseLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        values.Add(current.ToString());
        return values;
    }

    private enum CsvImportFormat
    {
        Unknown,
        Goodreads,
        Letterboxd
    }
}
