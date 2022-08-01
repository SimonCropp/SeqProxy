using System.Security.Claims;

class PrefixBuilder
{
    Func<string, string> scrubClaimType;
    string prefix;

    public PrefixBuilder(string application, Version version, Func<string, string> scrubClaimType, string server, string user)
    {
        this.scrubClaimType = scrubClaimType;
        prefix = $"{{'Application':'{application.AsJson()}','ApplicationVersion':'{version.ToString().AsJson()}','Server':'{server.AsJson()}','User':'{user.AsJson()}',";
    }

    public string Build(ClaimsPrincipal user, string? userAgent, string? referrer, string id)
    {
        var builder = new StringBuilder(prefix);
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