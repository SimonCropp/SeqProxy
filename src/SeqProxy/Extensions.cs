using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;

static class Extensions
{
    public static string AsJson(this string value)
    {
        return HttpUtility.JavaScriptStringEncode(value);
    }

    public static string GetUserAgent(this HttpRequest request)
    {
        if (request.Headers.TryGetValue("User-Agent", out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }
}