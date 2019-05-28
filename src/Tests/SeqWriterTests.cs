using System;
using System.Threading;
using System.Threading.Tasks;
using ObjectApproval;
using SeqProxy;
using Xunit;
using Xunit.Abstractions;

public class SeqWriterTests :
    XunitLoggingBase
{
    [Fact]
    public async Task Simple()
    {
        var httpFactory = new MockClientFactory();
        var writer = new SeqWriter(httpFactory, "http://theSeqUrl", "theAppName", new Version(1, 2), "theApiKey", false);
        var mockRequest = new MockRequest("{'@mt':'Simple Message'}");
        await writer.Handle(ClaimsBuilder.Build(), mockRequest, new MockResponse());
        ObjectApprover.VerifyWithJson(httpFactory.Client.Requests, s => s.Replace(Environment.MachineName, "TheMachineName"));
    }

    public SeqWriterTests(ITestOutputHelper output) :
        base(output)
    {
    }
}