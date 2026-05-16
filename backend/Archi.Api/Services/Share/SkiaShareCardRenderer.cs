using SkiaSharp;

namespace Archi.Api.Services.Share;

public sealed class SkiaShareCardRenderer : IShareCardRenderer
{
    private const int Width = 1080;
    private const int Height = 1920;

    public byte[] Render(ShareCardModel model)
    {
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(12, 12, 14));

        using var backgroundPaint = new SKPaint();
        var colors = new[] { new SKColor(28, 28, 36), new SKColor(12, 12, 14) };
        backgroundPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(Width, Height),
            colors,
            null,
            SKShaderTileMode.Clamp);
        canvas.DrawRect(new SKRect(0, 0, Width, Height), backgroundPaint);

        using var accentPaint = new SKPaint { Color = new SKColor(99, 102, 241), IsAntialias = true };
        canvas.DrawRoundRect(new SKRect(64, 120, 340, 188), 24, 24, accentPaint);

        using var categoryPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextSize = 42,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
        };
        canvas.DrawText(model.Category.ToUpperInvariant(), 96, 168, categoryPaint);

        using var titlePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            TextSize = 72,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
        };
        DrawWrappedText(canvas, model.Title, 64, 280, Width - 128, titlePaint, 3);

        using var metaPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 210),
            IsAntialias = true,
            TextSize = 40,
            Typeface = SKTypeface.FromFamilyName("Segoe UI")
        };

        var metaY = 520f;
        if (model.Metadata.Year is > 0)
        {
            canvas.DrawText($"Year {model.Metadata.Year}", 64, metaY, metaPaint);
            metaY += 56;
        }

        if (!string.IsNullOrWhiteSpace(model.Metadata.Author))
        {
            canvas.DrawText(model.Metadata.Author, 64, metaY, metaPaint);
            metaY += 56;
        }

        if (!string.IsNullOrWhiteSpace(model.Metadata.Platform))
        {
            canvas.DrawText(model.Metadata.Platform, 64, metaY, metaPaint);
            metaY += 56;
        }

        using var userPaint = new SKPaint
        {
            Color = new SKColor(160, 160, 170),
            IsAntialias = true,
            TextSize = 36,
            Typeface = SKTypeface.FromFamilyName("Segoe UI")
        };
        canvas.DrawText($"@{model.Username}", 64, Height - 160, userPaint);

        using var brandPaint = new SKPaint
        {
            Color = new SKColor(99, 102, 241),
            IsAntialias = true,
            TextSize = 48,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
        };
        canvas.DrawText("ARCHI", 64, Height - 96, brandPaint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static void DrawWrappedText(
        SKCanvas canvas,
        string text,
        float x,
        float y,
        float maxWidth,
        SKPaint paint,
        int maxLines)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var line = string.Empty;
        var lineIndex = 0;
        var currentY = y;

        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(line) ? word : $"{line} {word}";
            if (paint.MeasureText(candidate) > maxWidth && !string.IsNullOrEmpty(line))
            {
                canvas.DrawText(line, x, currentY, paint);
                line = word;
                currentY += paint.TextSize + 12;
                lineIndex++;
                if (lineIndex >= maxLines)
                {
                    return;
                }
            }
            else
            {
                line = candidate;
            }
        }

        if (!string.IsNullOrEmpty(line) && lineIndex < maxLines)
        {
            canvas.DrawText(line, x, currentY, paint);
        }
    }
}
