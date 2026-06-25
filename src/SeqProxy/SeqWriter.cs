using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace SeqProxy;

/// <summary>
/// Handles reads a log message from <see cref="HttpRequest"/> and forwarding it to Seq.
/// </summary>
public class SeqWriter
{
    Func<HttpClient> httpClientFunc;
    Uri url;
    string? apiKey;
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
    /// <param name="apiKey">The Seq api key to use. Sent as an `X-Seq-ApiKey` header when writing log entries.</param>
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
        this.apiKey = apiKey;
        url = GetSeqUrl(seqUrl);
        prefixBuilder = new(application, version, scrubClaimType, server, user);
    }

    static Uri GetSeqUrl(string seqUrl) =>
        new(new Uri(seqUrl), "api/events/raw");

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
            // Send the Seq api key as a header rather than in the URL query string, so it is not
            // exposed in request logs and does not need URL-encoding.
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            if (apiKey is not null)
            {
                request.Headers.Add("X-Seq-ApiKey", apiKey);
            }

            using var seqResponse = await httpClient.SendAsync(request, cancel);
            response.Headers["SeqProxyId"] = id;

            if (seqResponse.IsSuccessStatusCode)
            {
                response.StatusCode = (int)seqResponse.StatusCode;
                // Forward Seq's success body (e.g. MinimumLevelAccepted, used by clients for level control).
                await seqResponse.Content.CopyToAsync(response.Body, cancel);
            }
            else
            {
                // Don't relay Seq's error status or body to the (possibly anonymous) client; that
                // would disclose internal Seq details (version, config, ingestion errors).
                response.StatusCode = StatusCodes.Status502BadGateway;
            }
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