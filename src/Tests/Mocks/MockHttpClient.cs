public class MockHttpClient : HttpClient
{
    public List<LoggedRequest> Requests = [];

    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, Cancel cancel)
    {
        // Captured before reading: a streamed (chunked) content has no precomputed length.
        var contentLength = request.Content!.Headers.ContentLength;
        var content = await request.Content!.ReadAsStringAsync(cancel);
        Requests.Add(new(content) { ContentLength = contentLength });
        return new(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"MinimumLevelAccepted\":\"Information\"}")
        };
    }
}