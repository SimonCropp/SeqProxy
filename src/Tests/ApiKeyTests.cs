// Regression tests for finding #4: the Seq api key must be sent as an X-Seq-ApiKey header,
// not in the URL query string (where it leaks into request logs and needs URL-encoding).
public class ApiKeyTests
{
    static async Task<LoggedRequest> Handle(string? apiKey)
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName",
            new(1, 2),
            apiKey,
            _ => _,
            "theServer",
            "theUser");
        await writer.Handle(new(), new MockRequest("{'@mt':'Message'}"), new MockResponse());
        return httpClient.Requests.Single();
    }

    [Fact]
    public async Task ApiKeyIsSentAsHeaderNotUrl()
    {
        var logged = await Handle("TheApiKey");

        Assert.Equal("TheApiKey", logged.ApiKey);
        Assert.Empty(logged.Uri!.Query);
    }

    [Fact]
    public async Task NoApiKeyHeaderWhenNotConfigured()
    {
        var logged = await Handle(null);

        Assert.Null(logged.ApiKey);
        Assert.Empty(logged.Uri!.Query);
    }
}
