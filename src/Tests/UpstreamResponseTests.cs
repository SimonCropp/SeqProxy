// Regression tests for finding #5: the proxy must not relay Seq's raw error status/body to the
// (possibly anonymous) client, since that discloses internal Seq details. Successful responses
// (e.g. MinimumLevelAccepted, used for client-side level control) are still forwarded.
public class UpstreamResponseTests
{
    static async Task<MockResponse> Handle(MockHttpClient httpClient)
    {
        var writer = new SeqWriter(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName",
            new(1, 2),
            "theApiKey",
            _ => _,
            "theServer",
            "theUser");
        var response = new MockResponse();
        await writer.Handle(new(), new MockRequest("{'@mt':'Message'}"), response);
        return response;
    }

    static string ReadBody(MockResponse response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task SuccessResponseIsForwarded()
    {
        var response = await Handle(new());

        Assert.Equal(200, response.StatusCode);
        // Seq's success body is preserved so client-side level control keeps working.
        Assert.Contains("MinimumLevelAccepted", ReadBody(response));
    }

    [Fact]
    public async Task SeqErrorIsNotLeakedToClient()
    {
        var httpClient = new MockHttpClient
        {
            ResponseStatus = HttpStatusCode.Unauthorized,
            ResponseBody = "Seq error: invalid API key for instance v2024.1"
        };

        var response = await Handle(httpClient);

        // Generic gateway error; Seq's status and body are not disclosed.
        Assert.Equal(502, response.StatusCode);
        Assert.Empty(ReadBody(response));
        // The correlation id is still returned.
        Assert.True(response.Headers.ContainsKey("SeqProxyId"));
    }
}
