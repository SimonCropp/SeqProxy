using System.Text.Json;

static class Extensions
{
    public static string AsJson(this string value) =>
        JavaScriptEncoder.UnsafeRelaxedJsonEscaping.Encode(value);

    public static void WriteEscaped(this StringBuilder builder, string value)
    {
        var encode = JsonEncodedText.Encode(value, JavaScriptEncoder.UnsafeRelaxedJsonEscaping);
        builder.Append(encode);
    }
    public static void WriteEscaped(this StringBuilder builder, CharSpan value)
    {
        var encode = JsonEncodedText.Encode(value, JavaScriptEncoder.UnsafeRelaxedJsonEscaping);
        builder.Append(encode);
    }

    public static string? GetUserAgent(this HttpRequest request)
    {
        if (request.Headers.TryGetValue(HeaderNames.UserAgent, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    public static string? GetReferer(this HttpRequest request)
    {
        if (request.Headers.TryGetValue(HeaderNames.Referer, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }
}