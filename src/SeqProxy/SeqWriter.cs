using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace SeqProxy;

/// <summary>
/// Handles reads a log message from <see cref="HttpRequest"/> and forwarding it to Seq.
/// </summary>
public class SeqWriter
{
    Func<HttpClient> httpClientFunc;
    Uri url;
    PrefixBuilder prefixBuilder;
    static MediaTypeHeaderValue contentType = new("application/vnd.serilog.clef", Encoding.UTF8.WebName);

    /// <summary>
    /// Initializes a new instance of <see cref="SeqWriter"/>
    /// </summary>
    /// <param name="httpClientFunc">Builds a <see cref="HttpClient"/> for writing log entries to Seq.</param>
    /// <param name="seqUrl">The Seq api url.</param>
    /// <param name="application">The application name.</param>
    /// <param name="version">The application version.</param>
    /// <param name="apiKey">The Seq api key to use. Will be appended to <paramref name="seqUrl"/> when writing log entries.</param>
    /// <param name="scrubClaimType">Scrubber for claim types. If null then <see cref="DefaultClaimTypeScrubber.Scrub"/> will be used.</param>
    /// <param name="server">The value to use for the Seq `Server` property.</param>
    /// <param name="user">The value to use for the Seq `User` property</param>
    public SeqWriter(
        Func<HttpClient> httpClientFunc,
        string seqUrl,
        string application,
        Version version,
        string? apiKey,
        Func<string, string> scrubClaimType,
        string server,
        string user)
    {
        Guard.AgainstEmpty(apiKey, nameof(apiKey));
        Guard.AgainstNullOrEmpty(application, nameof(application));
        Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
        this.httpClientFunc = httpClientFunc;
        url = GetSeqUrl(seqUrl, apiKey);
        prefixBuilder = new(application, version, scrubClaimType, server, user);
    }

    static Uri GetSeqUrl(string seqUrl, string? apiKey)
    {
        var baseUri = new Uri(seqUrl);
        string uri;
        if (apiKey is null)
        {
            uri = "api/events/raw";
        }
        else
        {
            uri = $"api/events/raw?apiKey={apiKey}";
        }

        return new(baseUri, uri);
    }

    /// <summary>
    /// Reads a log message from <paramref name="request"/> and forwards it to Seq.
    /// </summary>
    public virtual async Task Handle(ClaimsPrincipal user, HttpRequest request, HttpResponse response, Cancel cancel = default)
    {
        ApiKeyValidator.ThrowIfApiKeySpecified(request);
        var builder = new StringBuilder();

        var utcNow = DateTime.UtcNow;
        var id = BuildId(utcNow);
        var prefix = prefixBuilder.Build(user, request.GetUserAgent(), request.GetReferer(), id);
        using (var reader = new StreamReader(request.Body))
        {
            while (await reader.ReadLineAsync(cancel) is { } line)
            {
                ValidateLine(line);

                builder.Append(prefix);
                if (!line.Contains("\"@t\"") &&
                    !line.Contains("'@t'"))
                {
                    builder.Append($"'@t':'{utcNow:o}',");
                }

                builder.Append(line, 1, line.Length - 1);
                builder.AppendLine();
            }
        }

        await Write(builder.ToString(), response, id, cancel);
    }

    static string BuildId(DateTime utcNow)
    {
        #region BuildId

        var startOfYear = new DateTime(utcNow.Year, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var ticks = utcNow.Ticks - startOfYear.Ticks;
        var id = ticks.ToString("x");

        #endregion

        return id;
    }

    async Task Write(string payload, HttpResponse response, string id, Cancel cancel)
    {
        var httpClient = httpClientFunc();
        try
        {
            using var content = new StringContent(payload);
            content.Headers.ContentType = contentType;
            using var seqResponse = await httpClient.PostAsync(url, content, cancel);
            response.StatusCode = (int)seqResponse.StatusCode;
            response.Headers["SeqProxyId"] = id;

            await seqResponse.Content.CopyToAsync(response.Body, cancel);
        }
        catch (TaskCanceledException)
        {
        }
    }

    static void ValidateLine(string line)
    {
        if (line.Length == 0)
        {
            throw new("Blank lines are not allowed.");
        }

        if (line.StartsWith("{'") ||
            line.StartsWith(@"{"""))
        {
            return;
        }

        throw new($"Expected line to start with `{{'` or `{{\"`. Line: {line}");
    }
}