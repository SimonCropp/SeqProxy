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
        var content = $$"""{"@t":"{{timestamp}}","@mt":"Hello, {User}","User":"John"}""";
        return WriteAndVerify(content);
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
        var content = """
                      {'@mt':'Message1'}
                      {'@mt':'Message2'}
                      """;
        return WriteAndVerify(content);
    }

    static async Task WriteAndVerify(string content, string url = "/api/events/raw")
    {
        using var server = TestServerBuilder.Build();
        using var client = server.CreateClient();
        try
        {
            client.DefaultRequestHeaders.Add("User-Agent", "TheUserAgent");
            using var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            await server.Host.StopAsync();
        }
    }
}