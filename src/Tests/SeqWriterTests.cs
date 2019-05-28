﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalTests;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using ObjectApproval;
using SeqProxy;
using Xunit;
using Xunit.Abstractions;

public class SeqWriterTests :
    XunitLoggingBase
{
    [Fact]
    public async Task Multiple()
    {
        var httpFactory = new MockClientFactory();
        var writer = new SeqWriter(httpFactory, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", false, s => s);
        var request = new MockRequest(
            @"{'@t':'2019-05-28','@mt':'Message1'}
{'@t':'2019-05-29','@mt':'Message2'}");
        await writer.Handle(ClaimsBuilder.Build(), request, new MockResponse());
        Verify(httpFactory);
    }

    static void Verify(MockClientFactory httpFactory)
    {
        ObjectApprover.VerifyWithJson(httpFactory.Client.Requests.Single().Body.Split(Environment.NewLine), Scrubber.Scrub);
    }

    [Fact]
    public async Task Simple()
    {
        var httpFactory = new MockClientFactory();
        var writer = new SeqWriter(httpFactory, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", false, s => s);
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}");
        await writer.Handle(ClaimsBuilder.Build(), request, new MockResponse());
        Verify(httpFactory);
    }

    [Fact]
    public async Task ApiKeyQueryString()
    {
        var httpFactory = new MockClientFactory();
        var writer = new SeqWriter(httpFactory, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", false, s => s);
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}")
        {
            Query = new QueryCollection(
                new Dictionary<string, StringValues>()
                {
                    {"apiKey", "foo"}
                })
        };

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        Approvals.Verify(exception.Message);
    }

    [Fact]
    public async Task ApiKeyHeaderString()
    {
        var httpFactory = new MockClientFactory();
        var writer = new SeqWriter(httpFactory, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", false, s => s);
        var request = new MockRequest("{'@t':'2019-05-28','@mt':'Simple Message'}")
        {
            Headers =
            {
                {"X-Seq-ApiKey", "foo"}
            }
        };

        var user = ClaimsBuilder.Build();
        var exception = await Assert.ThrowsAsync<Exception>(() => writer.Handle(user, request, new MockResponse()));
        Approvals.Verify(exception.Message);
    }

    public SeqWriterTests(ITestOutputHelper output) :
        base(output)
    {
    }
}