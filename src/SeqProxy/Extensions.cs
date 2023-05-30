static class Extensions
{
    public static string AsJson(this string value) =>
        JavaScriptEncoder.UnsafeRelaxedJsonEscaping.Encode(value);

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