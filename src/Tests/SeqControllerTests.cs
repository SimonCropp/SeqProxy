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
    public Task LogClassic()
    {
        var timestamp = DateTime.Now.ToString("o");
        var content = $@"
{{
  Events:[
    {{
      Timestamp: '{timestamp}',
      MessageTemplate: 'LogClassic test. Property: {{@property1}}',
      Level: 'Fatal',
      Properties: {{
        property1: 'some message'
      }}
    }}
  ]
}}";

        return WriteAndVerify(content, false);
    }

    static async Task WriteAndVerify(string content, bool compact)
    {
        using (var server = TestServerBuilder.Build())
        using (var client = server.CreateClient())
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "TheUserAgent");
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var apiEventsRaw = "/api/events/raw";
                if (compact)
                {
                    apiEventsRaw += "?clef";
                }
                var httpResponseMessage = await client.PostAsync(apiEventsRaw, httpContent);
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