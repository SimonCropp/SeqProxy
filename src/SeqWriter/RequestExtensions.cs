using System.Linq;
using Microsoft.AspNetCore.Http;

static class RequestExtensions
{
    public static string GetUserAgent(this HttpRequest request)
    {
        if (request.Headers.TryGetValue("User-Agent", out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }
}