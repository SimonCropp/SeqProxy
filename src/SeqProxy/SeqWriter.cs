using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace SeqProxy
{
    public class SeqWriter
    {
        IHttpClientFactory httpFactory;
        bool swallowSeqExceptions;
        static ILogger logger = Log.ForContext(typeof(SeqWriter));
        static byte[] defaultResponse = Encoding.UTF8.GetBytes("{\"MinimumLevelAccepted\":\"Information\"}");
        string url;
        PrefixBuilder prefixBuilder;

        internal SeqWriter()
        {
        }

        public SeqWriter(
            IHttpClientFactory httpFactory,
            string seqUrl,
            string appName,
            Version version,
            string apiKey,
            bool swallowSeqExceptions,
            Func<string, string> scrubClaimType)
        {
            Guard.AgainstEmpty(apiKey, nameof(apiKey));
            Guard.AgainstNullOrEmpty(appName, nameof(appName));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(httpFactory, nameof(httpFactory));
            Guard.AgainstNull(scrubClaimType, nameof(scrubClaimType));
            this.httpFactory = httpFactory;
            this.swallowSeqExceptions = swallowSeqExceptions;
            var baseUri = new Uri(seqUrl);
            var apiUrl = new Uri(baseUri, "api/events/raw?apiKey={apiKey}");
            url = apiUrl.ToString();
            prefixBuilder = new PrefixBuilder(appName, version, scrubClaimType);
        }

        public virtual async Task Handle(ClaimsPrincipal user, HttpRequest request, HttpResponse response, CancellationToken cancellation = default)
        {
            ThrowIfApiKeySpecified(request);
            var builder = new StringBuilder();
            var prefix = prefixBuilder.Build(user, request.GetUserAgent(), request.GetReferer());
            using (var streamReader = new StreamReader(request.Body))
            {
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    ValidateLine(line);

                    builder.Append(prefix);
                    if (!line.Contains("\"@t\"") &&
                        !line.Contains("'@t'"))
                    {
                        builder.Append($@"'@t':'{DateTime.UtcNow:o}',");
                    }

                    builder.Append(line, 1, line.Length - 1);
                    builder.AppendLine();
                }
            }

            await Write(builder.ToString(), response, cancellation);
        }

        static void ValidateLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new Exception("Blank lines are not allowed.");
            }

            if (line[0] != '{')
            {
                throw new Exception($"Expected first char of line to be a '{{'. Line: {line}");
            }
        }

        static void ThrowIfApiKeySpecified(HttpRequest request)
        {
            if (request.Query.ContainsKey("apiKey"))
            {
                throw new Exception("apiKey is not allowed.");
            }

            if (request.Headers.ContainsKey("X-Seq-ApiKey"))
            {
                throw new Exception("apiKey is not allowed.");
            }
        }

        async Task Write(string payload, HttpResponse response, CancellationToken cancellation)
        {
            var httpClient = httpFactory.CreateClient("SeqProxy");
            try
            {
                using (var content = new StringContent(payload, Encoding.UTF8, "application/vnd.serilog.clef"))
                using (var seqResponse = await httpClient.PostAsync(url, content, cancellation))
                {
                    response.StatusCode = (int) seqResponse.StatusCode;
                    await seqResponse.Content.CopyToAsync(response.Body);
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, $"Failed to write to Seq: {url}");
                if (swallowSeqExceptions)
                {
                    response.StatusCode = (int) HttpStatusCode.Created;
                    await response.Body.WriteAsync(defaultResponse, 0, defaultResponse.Length, cancellation);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}