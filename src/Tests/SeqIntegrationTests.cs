using System.Net.Http;
using VerifyXunit;
using Xunit;

[UsesVerify]
[Trait("Category", "Integration")]
public class SeqIntegrationTests
{
    [Fact]
    public Task Log()
    {
        var timestamp = DateTime.Now.ToString("o");
        var content = $"{{'@t':'{timestamp}','@mt':'Hello, {{User}}','User':'John'}}";
        return WriteAndVerify(content);
    }

    [Fact]
    public Task LogWithDoubleQuotes()
    {
        var timestamp = DateTime.Now.ToString("o");
        var content = $@"{{""@t"":""{timestamp}"",""@mt"":""Hello, {{User}}"",""User"":""John""}}";
        return WriteAndVerify(content);
    }

    [Fact]
    public async Task OldFormat1()
    {
        var content = @"{""Events"": [{""Level"": ""Error"",""MessageTemplate"": ""The Message""}]}";
        var exception = await Assert.ThrowsAsync<Exception>(() => WriteAndVerify(content));
        await Verifier.Verify(exception.Message);
    }

    [Fact]
    public async Task OldFormat2()
    {
        var content = "{'Events': [{'Level': 'Error','MessageTemplate': 'The Message'}]}";
        var exception = await Assert.ThrowsAsync<Exception>(() => WriteAndVerify(content));
        await Verifier.Verify(exception.Message);
    }

    [Fact]
    public async Task OldFormat3()
    {
        var content = @"{
  ""Events"": [{
    ""Timestamp"": ""2015-05-09T22:09:08.12345+10:00"",
    ""Level"": ""Warning"",
    ""MessageTemplate"": ""Disk space is low on {Drive}"",
    ""Properties"": {
      ""Drive"": ""C:"",
      ""MachineName"": ""nblumhardt-rmbp""
    }
  }]
}";
        var exception = await Assert.ThrowsAsync<Exception>(() => WriteAndVerify(content));
        await Verifier.Verify(exception.Message);
    }

    [Fact]
    public Task LogToController()
    {
        var timestamp = DateTime.Now.ToString("o");
        var content = $"{{'@t':'{timestamp}','@mt':'Hello, {{User}}','User':'John'}}";
        return WriteAndVerify(content, "/seqcontroller");
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
        using var server = TestServerBuilder.Build();
        using var client = server.CreateClient();
        try
        {
            client.DefaultRequestHeaders.Add("User-Agent", "TheUserAgent");
            StringContent httpContent = new(content, Encoding.UTF8, "application/json");
            var httpResponseMessage = await client.PostAsync(url, httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();
        }
        finally
        {
            await server.Host.StopAsync();
        }
    }
}