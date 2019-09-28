﻿using System;
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
        prefix = $"{{'Application':'{application.AsJson()}','ApplicationVersion':'{version.ToString().AsJson()}','Server':'{Environment.MachineName.AsJson()}',";
    }

    public string Build(ClaimsPrincipal user, string? userAgent, string? referrer)
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

        if (userAgent != null)
        {
            builder.Append($"'UserAgent':'{userAgent.AsJson()}',");
        }
        if (referrer != null)
        {
            builder.Append($"'Referrer':'{referrer.AsJson()}',");
        }
        return builder.ToString();
    }
}