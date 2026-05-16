using System.Globalization;
using System.Text;

namespace Archi.Api.Services.Feed;

public static class FeedCursor
{
    private const char Separator = '|';

    public static string Encode(DateTimeOffset createdAt, Guid id)
    {
        var payload = $"{createdAt.UtcDateTime:O}{Separator}{id:D}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }

    public static bool TryDecode(string? cursor, out DateTimeOffset createdAt, out Guid id)
    {
        createdAt = default;
        id = default;

        if (string.IsNullOrWhiteSpace(cursor))
        {
            return false;
        }

        try
        {
            var bytes = Convert.FromBase64String(cursor.Trim());
            var payload = Encoding.UTF8.GetString(bytes);
            var separatorIndex = payload.LastIndexOf(Separator);
            if (separatorIndex <= 0)
            {
                return false;
            }

            var createdAtRaw = payload[..separatorIndex];
            var idRaw = payload[(separatorIndex + 1)..];

            if (!DateTimeOffset.TryParse(
                    createdAtRaw,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out createdAt))
            {
                return false;
            }

            return Guid.TryParse(idRaw, out id);
        }
        catch
        {
            return false;
        }
    }
}
