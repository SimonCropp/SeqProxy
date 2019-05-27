using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace SeqProxy
{
    public class Poster
    {
        static ILogger logger = Log.ForContext(typeof(Poster));
        static byte[] defaultResponse = Encoding.ASCII.GetBytes("{\"MinimumLevelAccepted\":\"Information\"}");
        HttpClient httpClient = new HttpClient();
        string url;
        SuffixBuilder suffixBuilder;

        public Poster(string seqUrl, string appName, Version version, string apiKey)
        {
            url = $"{seqUrl}/api/events/raw?apiKey={apiKey}";
            suffixBuilder = new SuffixBuilder(appName, version);
        }

        public virtual async Task Handle(ClaimsPrincipal user, HttpRequest request, HttpResponse response)
        {
            var builder = new StringBuilder();
            var suffix = suffixBuilder.Build(user, request.GetUserAgent());
            using (var streamReader = new StreamReader(request.Body))
            {
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        throw new Exception();
                    }
                    if (line.Last() != '}')
                    {
                        throw new Exception();
                    }

                    builder.Append(line);
                    builder.Insert(builder.Length - 1, suffix);
                }
            }

            await Write(builder.ToString(), response);
        }

        async Task Write(string payload, HttpResponse response)
        {
            try
            {
                using (var content = new StringContent(payload, Encoding.UTF8, "application/vnd.serilog.clef"))
                using (var seqResponse = await httpClient.PostAsync(url, content))
                {
                    response.StatusCode =  (int)seqResponse.StatusCode;
                    await seqResponse.Content.CopyToAsync(response.Body);
                }
            }
            catch (Exception exception)
            {
                await LogAndWriteDefault(response, exception);
                return;
            }
        }

        Task LogAndWriteDefault(HttpResponse httpResponse, Exception exception)
        {
            logger.Error(exception, $"Failed to write to Seq: {url}");
            httpResponse.StatusCode = (int) HttpStatusCode.Created;
            return httpResponse.Body.WriteAsync(defaultResponse, 0, defaultResponse.Length);
        }
    }
}