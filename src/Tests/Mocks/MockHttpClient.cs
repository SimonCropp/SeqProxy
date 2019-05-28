using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class MockHttpClient : HttpClient
{
    public List<LoggedRequest> Requests = new List<LoggedRequest>();

    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(
            new LoggedRequest
            {
                Body = await request.Content.ReadAsStringAsync()
            });
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"MinimumLevelAccepted\":\"Information\"}")
        };
    }
}