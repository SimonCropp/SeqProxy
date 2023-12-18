class PrefixBuilder(string application, Version version, Func<string, string> scrubClaimType, string server, string user)
{
    string prefix = $"{{'Application':'{application.AsJson()}','ApplicationVersion':'{version.ToString().AsJson()}','Server':'{server.AsJson()}','User':'{user.AsJson()}',";

    public string Build(ClaimsPrincipal user, string? userAgent, string? referrer, string id)
    {
        var builder = new StringBuilder(prefix);
        var writer = new StringWriter(builder);
        if (user.Claims.Any())
        {
            builder.Append("'Claims':{");
            foreach (var claim in user.Claims)
            {
                var claimType = scrubClaimType(claim.Type);
                builder.Append('\'');
                writer.WriteEscaped(claimType);
                builder.Append("':'");
                writer.WriteEscaped(claim.Value);
                builder.Append("',");
            }

            builder.Length -= 1;
            builder.Append("},");
        }

        builder.Append($"'SeqProxyId':'{id}',");
        if (userAgent is not null)
        {
            builder.Append("'UserAgent':'");
            writer.WriteEscaped(userAgent);
            builder.Append("',");
        }

        if (referrer is not null)
        {
            builder.Append("'Referrer':'");
            writer.WriteEscaped(referrer);
            builder.Append("',");
        }

        return builder.ToString();
    }
}