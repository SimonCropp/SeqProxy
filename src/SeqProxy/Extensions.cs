using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

static class Extensions
{
    public static string AsJson(this string value)
    {
        return HttpUtility.JavaScriptStringEncode(value);
    }

    public static string GetUserAgent(this HttpRequest request)
    {
        if (request.Headers.TryGetValue(HeaderNames.UserAgent, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    public static string GetReferer(this HttpRequest request)
    {
        if (request.Headers.TryGetValue(HeaderNames.Referer, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }
}