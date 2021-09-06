using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

class PrefixBuilder
{
    Func<string, string> scrubClaimType;
    string prefix;

    public PrefixBuilder(string application, Version version, Func<string, string> scrubClaimType)
    {
        this.scrubClaimType = scrubClaimType;
        var machine = Environment.MachineName;
        prefix = $"{{'Application':'{application.AsJson()}','ApplicationVersion':'{version.ToString().AsJson()}','Server':'{machine.AsJson()}',";
    }

    public string Build(ClaimsPrincipal user, string? userAgent, string? referrer, string id)
    {
        StringBuilder builder = new(prefix);
        if (user.Claims.Any())
        {
            builder.Append("'Claims':{");
            foreach (var claim in user.Claims)
            {
                builder.Append($"'{scrubClaimType(claim.Type).AsJson()}':'{claim.Value.AsJson()}',");
            }

            builder.Length -= 1;
            builder.Append("},");
        }

        builder.Append($"'SeqProxyId':'{id}',");
        if (userAgent is not null)
        {
            builder.Append($"'UserAgent':'{userAgent.AsJson()}',");
        }

        if (referrer is not null)
        {
            builder.Append($"'Referrer':'{referrer.AsJson()}',");
        }

        return builder.ToString();
    }
}