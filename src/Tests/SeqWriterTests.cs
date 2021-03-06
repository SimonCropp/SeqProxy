﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SeqProxy;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class SeqWriterTests
{
    [Fact]
    public async Task Multiple()
    {
        MockHttpClient httpClient = new();
        SeqWriter writer = new(() => httpClient, "http://theSeqUrl", "theAppName", new(1, 2), "theApiKey", s => s);
        MockRequest request = new(
            @"{'@t':'2019-05-28','@mt':'Message1'}
{'@t':'2019-05-29','@mt':'Message2'}");
        var user = ClaimsBuilder.Build();
        await writer.Handle(user, request, new MockResponse());
        await Verify(httpClient);
    }

    static Task Verify(MockHttpClient httpClient)
    {
        var lines = httpClient.Requests.Single()
            .Body
            .Split(',')
            .Where(x => !x.Contains("SeqProxyId"));

        return Verifier.Verify(string.Join("," + Environment.NewLine, lines));
    }

    [Fact]
    public async Task Simple()
    {
        MockHttpClient httpClient = new();
        SeqWriter writer = new(() => httpClient, "http://theSeqUrl", "theAppName", new(1, 2), "theApiKey", s => s);
        MockRequest request = new("{'@t':'2019-05-28','@mt':'Simple Message'}");
        var user = ClaimsBuilder.Build();
        await writer.Handle(user, request, new MockResponse());
        await Verify(httpClient);
    }

    [Fact]
    public async Task ApiKeyQueryString()
    {
        MockHttpClient httpClient = new();
        SeqWriter writer = new(() => httpClient, "http://theSeqUrl", "theAppName", new(1, 2), "theApiKey", s => s);
        MockRequest request = new("{'@t':'2019-05-28','@mt':'Simple Message'}")
        {
            Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    {"apiKey", "foo"}
                })
        };

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        await Verifier.Verify(exception.Message);
    }

    [Fact]
    public async Task ApiKeyHeaderString()
    {
        MockHttpClient httpClient = new();
        SeqWriter writer = new(() => httpClient, "http://theSeqUrl", "theAppName", new(1, 2), "theApiKey", s => s);
        MockRequest request = new("{'@t':'2019-05-28','@mt':'Simple Message'}")
        {
            Headers =
            {
                {"X-Seq-ApiKey", "foo"}
            }
        };

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        await Verifier.Verify(exception.Message);
    }
}