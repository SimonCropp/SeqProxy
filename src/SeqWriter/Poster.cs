using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace SeqWriter
{
    public class Poster
    {
        static ILogger logger = Log.ForContext(typeof(Poster));
        static byte[] defaultResponse = Encoding.ASCII.GetBytes("{\"MinimumLevelAccepted\":\"Information\"}");
        HttpClient httpClient = new HttpClient();
        string url;

        public Poster(string seqUrl, string apiKey)
        {
            url = $"{seqUrl}/api/events/raw?apiKey={apiKey}";
        }

        public async Task Write(string payload, HttpResponse httpResponse)
        {
            int statusCode;
            byte[] buffer;
            try
            {
                using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                using (var response = await httpClient.PostAsync(url, content))
                {
                    response.EnsureSuccessStatusCode();
                    statusCode = (int) response.StatusCode;
                    buffer = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                await LogAndWriteDefault(httpResponse, exception);
                return;
            }

            httpResponse.StatusCode = statusCode;
            await httpResponse.Body.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        }

        Task LogAndWriteDefault(HttpResponse httpResponse, Exception exception)
        {
            logger.Error(exception, $"Failed to write to Seq: {url}");
            httpResponse.StatusCode = (int) HttpStatusCode.Created;
            return httpResponse.Body.WriteAsync(defaultResponse, 0, defaultResponse.Length);
        }
    }
}