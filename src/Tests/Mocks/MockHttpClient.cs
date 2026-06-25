public class MockHttpClient : HttpClient
{
    public List<LoggedRequest> Requests = [];
    public HttpStatusCode ResponseStatus = HttpStatusCode.OK;
    public string ResponseBody = "{\"MinimumLevelAccepted\":\"Information\"}";

    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, Cancel cancel)
    {
        // Captured before reading: a streamed (chunked) content has no precomputed length.
        var contentLength = request.Content!.Headers.ContentLength;
        request.Headers.TryGetValues("X-Seq-ApiKey", out var apiKey);
        var content = await request.Content!.ReadAsStringAsync(cancel);
        Requests.Add(
            new(content)
            {
                ContentLength = contentLength,
                Uri = request.RequestUri,
                ApiKey = apiKey?.FirstOrDefault()
            });
        return new(ResponseStatus)
        {
            Content = new StringContent(ResponseBody)
        };
    }
}