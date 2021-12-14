using System.Net;

public class MockHttpClient : HttpClient
{
    public List<LoggedRequest> Requests = new();

    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var content = await request.Content!.ReadAsStringAsync(cancellationToken);
        Requests.Add(new(content));
        return new(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"MinimumLevelAccepted\":\"Information\"}")
        };
    }
}