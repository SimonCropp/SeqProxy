// Regression tests for finding #3: the transformed payload must be streamed to Seq, not
// buffered in memory, otherwise a large or amplifying request can exhaust memory.
public class StreamingTests
{
    static SeqWriter BuildWriter(MockHttpClient httpClient) =>
        new(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName",
            new(1, 2),
            "theApiKey",
            _ => _,
            "theServer",
            "theUser");

    [Fact]
    public async Task PayloadIsStreamedNotBuffered()
    {
        var httpClient = new MockHttpClient();
        var writer = BuildWriter(httpClient);
        var request = new MockRequest("{'@mt':'Message'}");

        await writer.Handle(new(), request, new MockResponse());

        // A streamed (chunked) request has no precomputed Content-Length. Buffering the whole
        // payload into a StringContent (the previous behaviour) would set one.
        Assert.Null(httpClient.Requests.Single().ContentLength);
    }

    [Fact]
    public async Task ManyLinesAreAllForwarded()
    {
        var httpClient = new MockHttpClient();
        var writer = BuildWriter(httpClient);
        var body = string.Join('\n', Enumerable.Range(0, 5000).Select(_ => "{'@mt':'Message'}"));
        var request = new MockRequest(body);

        await writer.Handle(new(), request, new MockResponse());

        // Every event is streamed through, none dropped.
        var forwarded = httpClient.Requests.Single().Body;
        Assert.Equal(5000, forwarded.Split("'@mt':'Message'}").Length - 1);
    }
}
