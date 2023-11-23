using System.Net;

public class MockHttpClient : HttpClient
{
    public List<LoggedRequest> Requests = [];

    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, Cancel cancel)
    {
        var content = await request.Content!.ReadAsStringAsync(cancel);
        Requests.Add(new(content));
        return new(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"MinimumLevelAccepted\":\"Information\"}")
        };
    }
}