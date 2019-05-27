using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;

class SuffixBuilder
{
    string suffix;

    public SuffixBuilder(string appName, Version version)
    {
        suffix = $",'AppName':'{appName.AsJson()}','AppVersion':'{version.ToString().AsJson()}','Server':'{Environment.MachineName.AsJson()}'";
    }

    public string Build(HttpRequest request, ClaimsPrincipal user)
    {
        var userAgent = request.GetUserAgent();
        return Build(user, userAgent);
    }

    public string Build(ClaimsPrincipal user, string userAgent)
    {
        var builder = new StringBuilder(suffix);
        if (user.Claims.Any())
        {
            builder.Append(",'Claims':{");
            foreach (var claim in user.Claims)
            {
                builder.Append($"'{claim.Type.AsJson()}':'{claim.Value.AsJson()}',");
            }

            builder.Length -= 1;
            builder.Append("}");
        }

        builder.Append($",'UserAgent':'{userAgent.AsJson()}'");
        return builder.ToString();
    }
}