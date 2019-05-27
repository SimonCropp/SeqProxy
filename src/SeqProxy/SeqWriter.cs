using System;
using System.IO;
using System.Linq;
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

        public SeqWriter(IHttpClientFactory httpFactory, string seqUrl, string appName, Version version, string apiKey, bool swallowSeqExceptions)
        {
            Guard.AgainstEmpty(apiKey, nameof(apiKey));
            Guard.AgainstNullOrEmpty(appName, nameof(appName));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(httpFactory, nameof(httpFactory));
            this.httpFactory = httpFactory;
            this.swallowSeqExceptions = swallowSeqExceptions;
            url = $"{seqUrl}/api/events/raw?apiKey={apiKey}";
            prefixBuilder = new PrefixBuilder(appName, version);
        }

        public virtual async Task Handle(ClaimsPrincipal user, HttpRequest request, HttpResponse response, CancellationToken cancellation)
        {
            var builder = new StringBuilder();
            var prefix = prefixBuilder.Build(user, request.GetUserAgent());
            using (var streamReader = new StreamReader(request.Body))
            {
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        throw new Exception("Blank lines are not allowed.");
                    }

                    if (line.First() != '{')
                    {
                        throw new Exception($"Expected first char of line to be a '{{'. Line: {line}");
                    }

                    builder.Append(prefix);
                    if (!line.Contains("\"@t\"") &&
                        !line.Contains("'@t'"))
                    {
                        builder.Append($@"'@t':'{DateTime.UtcNow:o}',");
                    }
                    builder.Append(line.Substring(1));
                }
            }

            await Write(builder.ToString(), response,cancellation);
        }

        async Task Write(string payload, HttpResponse response, CancellationToken cancellation)
        {
            var httpClient = httpFactory.CreateClient("SeqProxy");
            try
            {
                using (var content = new StringContent(payload, Encoding.UTF8, "application/vnd.serilog.clef"))
                using (var seqResponse = await httpClient.PostAsync(url, content, cancellation))
                {
                    response.StatusCode = (int)seqResponse.StatusCode;
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