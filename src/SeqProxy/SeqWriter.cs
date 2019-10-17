using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SeqProxy
{
    public class SeqWriter
    {
        Func<HttpClient> httpClientFunc;
        string url;
        PrefixBuilder prefixBuilder;

        public SeqWriter(
            Func<HttpClient> httpClientFunc,
            string seqUrl,
            string application,
            Version version,
            string apiKey,
            Func<string, string> scrubClaimType)
        {
            Guard.AgainstEmpty(apiKey, nameof(apiKey));
            Guard.AgainstNullOrEmpty(application, nameof(application));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(httpClientFunc, nameof(httpClientFunc));
            Guard.AgainstNull(scrubClaimType, nameof(scrubClaimType));
            this.httpClientFunc = httpClientFunc;
            url = GetSeqUrl(seqUrl, apiKey);
            prefixBuilder = new PrefixBuilder(application, version, scrubClaimType);
        }

        static string GetSeqUrl(string seqUrl, string apiKey)
        {
            var baseUri = new Uri(seqUrl);
            string uri;
            if (apiKey == null)
            {
                uri = "api/events/raw";
            }
            else
            {
                uri = $"api/events/raw?apiKey={apiKey}";
            }

            var apiUrl = new Uri(baseUri, uri);
            return apiUrl.ToString();
        }

        public virtual async Task Handle(ClaimsPrincipal user, HttpRequest request, HttpResponse response, CancellationToken cancellation = default)
        {
            ApiKeyValidator.ThrowIfApiKeySpecified(request);
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

        async Task Write(string payload, HttpResponse response, CancellationToken cancellation)
        {
            var httpClient = httpClientFunc();
            try
            {
                using var content = new StringContent(payload, Encoding.UTF8, "application/vnd.serilog.clef");
                using var seqResponse = await httpClient.PostAsync(url, content, cancellation);
                response.StatusCode = (int) seqResponse.StatusCode;
                await seqResponse.Content.CopyToAsync(response.Body);
            }
            catch (TaskCanceledException)
            {
            }
        }

        static void ValidateLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new Exception("Blank lines are not allowed.");
            }

            if (line.StartsWith(@"{{""Events"":"))
            {
                throw new Exception("Only compact format is supported supported");
            }

            if (line[0] != '{')
            {
                throw new Exception($"Expected first char of line to be a '{{'. Line: {line}");
            }
        }
    }
}