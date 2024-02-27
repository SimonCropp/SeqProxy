public class SeqWriterTests
{
    [Fact]
    public async Task Multiple()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName",
            new(1, 2),
            "theApiKey",
            _ => _,
            "theServer",
            "theUser");
        var request = new MockRequest(
            """
            {'@t':'2019-05-28','@mt':'Message1'}
            {'@t':'2019-05-29','@mt':'Message2'}
            """);
        var user = ClaimsBuilder.Build();
        await writer.Handle(user, request, new MockResponse());
        await Verify(httpClient);
    }

    static Task Verify(MockHttpClient httpClient)
    {
        var lines = httpClient.Requests.Single()
            .Body
            .Split(',')
            .Where(_ => !_.Contains("SeqProxyId"));

        return Verifier.Verify(string.Join(',' + Environment.NewLine, lines));
    }

    [Fact]
    public async Task Simple()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName", new(1, 2),
            "theApiKey",
            _ => _,
            "theServer",
            "theUser");
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}");
        var user = ClaimsBuilder.Build();
        await writer.Handle(user, request, new MockResponse());
        await Verify(httpClient);
    }

    [Fact]
    public async Task JsonEscapeClaim()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName", new(1, 2),
            "theApiKey",
            _ => "\"",
            "theServer",
            "theUser");
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}");
        var user = ClaimsBuilder.Build();
        await writer.Handle(user, request, new MockResponse());
        await Verify(httpClient);
    }

    [Fact]
    public async Task ApiKeyQueryString()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName",
            new(1, 2),
            "theApiKey",
            _ => _,
            "theServer", "theUser");
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}")
        {
            Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    {
                        "apiKey", "foo"
                    }
                })
        };

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        await Verifier.Verify(exception.Message);
    }

    [Fact]
    public async Task ApiKeyHeaderString()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(
            () => httpClient,
            "http://theSeqUrl",
            "theAppName", new(1, 2),
            "theApiKey",
            _ => _,
            "theServer",
            "theUser");
        // ReSharper disable once UseObjectOrCollectionInitializer
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}");
        request.Headers["X-Seq-ApiKey"] = "foo";

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        await Verifier.Verify(exception.Message);
    }
}