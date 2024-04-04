class PrefixBuilder(string application, Version version, ScrubClaimType scrubClaimType, string server, string user)
{
    string prefix = $"{{'Application':'{application.AsJson()}','ApplicationVersion':'{version.ToString().AsJson()}','Server':'{server.AsJson()}','User':'{user.AsJson()}',";

    public string Build(ClaimsPrincipal user, string? userAgent, string? referrer, string id)
    {
        var builder = new StringBuilder(prefix);
        if (user.Claims.Any())
        {
            builder.Append("'Claims':{");
            foreach (var claim in user.Claims)
            {
                var claimType = scrubClaimType(claim.Type);
                builder.Append('\'');
                builder.WriteEscaped(claimType);
                builder.Append("':'");
                builder.WriteEscaped(claim.Value);
                builder.Append("',");
            }

            builder.Length -= 1;
            builder.Append("},");
        }

        builder.Append($"'SeqProxyId':'{id}',");
        if (userAgent is not null)
        {
            builder.Append("'UserAgent':'");
            builder.WriteEscaped(userAgent);
            builder.Append("',");
        }

        if (referrer is not null)
        {
            builder.Append("'Referrer':'");
            builder.WriteEscaped(referrer);
            builder.Append("',");
        }

        return builder.ToString();
    }
}