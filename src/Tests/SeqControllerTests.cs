using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class SeqControllerTests :
    XunitLoggingBase
{
    [Fact]
    public Task Log()
    {
        var timestamp = DateTime.Now.ToString("o");
        var content = $@"{{'@t':'{timestamp}','@mt':'Hello, {{User}}','User':'John'}}";

        return WriteAndVerify(content);
    }

    static async Task RawPost(string content, bool compact)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5341")
        };
        client.DefaultRequestHeaders.Add("User-Agent", "TheUserAgent");
        var httpContent = new StringContent(content, Encoding.UTF8, "application/vnd.serilog.clef");
        var apiEventsRaw = "/api/events/raw";
        if (compact)
        {
            apiEventsRaw += "?clef";
        }

        var httpResponseMessage = await client.PostAsync(apiEventsRaw, httpContent);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    static async Task WriteAndVerify(string content)
    {
        using (var server = TestServerBuilder.Build())
        using (var client = server.CreateClient())
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "TheUserAgent");
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var httpResponseMessage = await client.PostAsync("/api/events/raw", httpContent);
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            finally
            {
                await server.Host.StopAsync();
            }
        }
    }

    public SeqControllerTests(ITestOutputHelper output) :
        base(output)
    {
    }
}