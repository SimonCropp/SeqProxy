using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

[Trait("Category", "Integration")]
public class SeqIntegrationTests :
    XunitLoggingBase
{
    [Fact]
    public Task Log()
    {
        var timestamp = DateTime.Now.ToString("o");
        var content = $@"{{'@t':'{timestamp}','@mt':'Hello, {{User}}','User':'John'}}";
        return WriteAndVerify(content);
    }

    [Fact]
    public Task LogToController()
    {
        var timestamp = DateTime.Now.ToString("o");
        var content = $@"{{'@t':'{timestamp}','@mt':'Hello, {{User}}','User':'John'}}";
        return WriteAndVerify(content,"/seqcontroller");
    }

    [Fact]
    public Task LogNoTimestamp()
    {
        var content = "{'@mt':'LogNoTimestamp'}";
        return WriteAndVerify(content);
    }

    [Fact]
    public Task LogMultiple()
    {
        var content = @"{'@mt':'Message1'}
{'@mt':'Message2'}";
        return WriteAndVerify(content);
    }

    static async Task WriteAndVerify(string content, string url = "/api/events/raw")
    {
        using (var server = TestServerBuilder.Build())
        using (var client = server.CreateClient())
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "TheUserAgent");
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var httpResponseMessage = await client.PostAsync(url, httpContent);
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            finally
            {
                await server.Host.StopAsync();
            }
        }
    }

    public SeqIntegrationTests(ITestOutputHelper output) :
        base(output)
    {
    }
}