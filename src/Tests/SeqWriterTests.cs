using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SeqProxy;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class SeqWriterTests :
    VerifyBase
{
    [Fact]
    public async Task Multiple()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(() => httpClient, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", s => s);
        var request = new MockRequest(
            @"{'@t':'2019-05-28','@mt':'Message1'}
{'@t':'2019-05-29','@mt':'Message2'}");
        await writer.Handle(ClaimsBuilder.Build(), request, new MockResponse());
        await Verify(httpClient);
    }

    Task Verify(MockHttpClient httpClient)
    {
        return Verify(httpClient.Requests.Single().Body.Split(Environment.NewLine));
    }

    [Fact]
    public async Task Simple()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(() => httpClient, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", s => s);
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}");
        await writer.Handle(ClaimsBuilder.Build(), request, new MockResponse());
        await Verify(httpClient);
    }

    [Fact]
    public async Task ApiKeyQueryString()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(() => httpClient, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", s => s);
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}")
        {
            Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    {"apiKey", "foo"}
                })
        };

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        await Verify(exception.Message);
    }

    [Fact]
    public async Task ApiKeyHeaderString()
    {
        var httpClient = new MockHttpClient();
        var writer = new SeqWriter(() => httpClient, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", s => s);
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}")
        {
            Headers =
            {
                {"X-Seq-ApiKey", "foo"}
            }
        };

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        await Verify(exception.Message);
    }

    public SeqWriterTests(ITestOutputHelper output) :
        base(output)
    {
    }
}