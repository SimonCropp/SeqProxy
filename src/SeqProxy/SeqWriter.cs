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
    static UTF8Encoding utf8NoBom = new(false);

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
        ScrubClaimType scrubClaimType,
        string server,
        string user)
    {
        Ensure.NotEmpty(apiKey);
        Ensure.NotNullOrEmpty(application);
        Ensure.NotNullOrEmpty(seqUrl);
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

        var utcNow = DateTime.UtcNow;
        var id = BuildId(utcNow);
        var prefix = prefixBuilder.Build(user, request.GetUserAgent(), request.GetReferer(), id);

        // Stream the transformed events straight to Seq so the full (potentially amplified)
        // payload is never buffered in memory.
        using var content = new ClefContent(request.Body, prefix, utcNow);
        content.Headers.ContentType = contentType;
        await Write(content, response, id, cancel);
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

    async Task Write(HttpContent content, HttpResponse response, string id, Cancel cancel)
    {
        var httpClient = httpClientFunc();
        try
        {
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

    // Streams the transformed CLEF events directly to Seq so the full payload is never
    // buffered in memory (prevents memory exhaustion from large or amplifying requests).
    // Validation runs per line as it streams, so a malformed or forged line part-way through
    // a multi-line request aborts the send after earlier lines have already been written.
    class ClefContent(Stream body, string prefix, DateTime utcNow) :
        HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context) =>
            SerializeToStreamAsync(stream, context, Cancel.None);

        protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context, Cancel cancel)
        {
            using var reader = new StreamReader(body);
            await using var writer = new StreamWriter(stream, utf8NoBom, 4096, leaveOpen: true);
            while (await reader.ReadLineAsync(cancel) is { } line)
            {
                ValidateLine(line);
                ReservedKeyValidator.ThrowIfReservedKey(line);

                await writer.WriteAsync(prefix);
                if (!line.Contains("\"@t\"") &&
                    !line.Contains("'@t'"))
                {
                    await writer.WriteAsync($"'@t':'{utcNow:o}',");
                }

                await writer.WriteAsync(line.AsMemory(1), cancel);
                await writer.WriteLineAsync();
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}