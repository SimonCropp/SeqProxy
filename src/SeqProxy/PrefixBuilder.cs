using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

class PrefixBuilder
{
    Func<string, string> scrubClaimType;
    string prefix;

    public PrefixBuilder(string appName, Version version, Func<string, string> scrubClaimType)
    {
        this.scrubClaimType = scrubClaimType;
        prefix = $"{{'AppName':'{appName.AsJson()}','AppVersion':'{version.ToString().AsJson()}','Server':'{Environment.MachineName.AsJson()}'";
    }

    public string Build(ClaimsPrincipal user, string userAgent)
    {
        var builder = new StringBuilder(prefix);
        if (user.Claims.Any())
        {
            builder.Append(",'Claims':{");
            foreach (var claim in user.Claims)
            {
                builder.Append($"'{scrubClaimType(claim.Type).AsJson()}':'{claim.Value.AsJson()}',");
            }

            builder.Length -= 1;
            builder.Append("}");
        }

        builder.Append($",'UserAgent':'{userAgent.AsJson()}',");
        return builder.ToString();
    }
}