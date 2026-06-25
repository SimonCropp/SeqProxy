using System.Security.Claims;
using Argon;

// Regression tests for the JSON/log-injection vulnerability (finding #1).
// Attacker-controlled values (User-Agent / Referer headers and claim values) are
// written into single-quoted JSON strings. They must be escaped so a value cannot
// break out of its string and inject or override properties in the event sent to Seq.
public class InjectionTests
{
    // If not escaped, the leading `'` closes the enclosing string and `,'Injected':'`
    // injects a sibling property into the event.
    const string Breakout = "evil','Injected':'pwned";

    static async Task<JObject> Handle(MockRequest request, ClaimsPrincipal user)
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
        await writer.Handle(user, request, new MockResponse());
        // The body posted to Seq is exactly what the backend would parse.
        return JObject.Parse(httpClient.Requests.Single().Body);
    }

    [Fact]
    public async Task UserAgentHeaderCannotInjectProperties()
    {
        var request = new MockRequest("{'@mt':'Message'}");
        request.Headers["User-Agent"] = Breakout;

        var json = await Handle(request, new());

        // The whole value stays inside UserAgent, and no breakout property is created.
        Assert.Equal(Breakout, (string?)json["UserAgent"]);
        Assert.Null(json["Injected"]);
    }

    [Fact]
    public async Task RefererHeaderCannotInjectProperties()
    {
        var request = new MockRequest("{'@mt':'Message'}");
        request.Headers["Referer"] = Breakout;

        var json = await Handle(request, new());

        Assert.Equal(Breakout, (string?)json["Referrer"]);
        Assert.Null(json["Injected"]);
    }

    [Fact]
    public async Task UserAgentHeaderCannotForgeTrustedField()
    {
        var request = new MockRequest("{'@mt':'Message'}");
        request.Headers["User-Agent"] = "evil','Server':'spoofed";

        var json = await Handle(request, new());

        // The server-stamped `Server` value cannot be overridden via the header.
        Assert.Equal("theServer", (string?)json["Server"]);
    }

    [Fact]
    public async Task ClaimValueCannotInjectProperties()
    {
        // `'}` also tries to close the Claims object and inject a top-level property.
        const string claimAttack = "evil'},'Injected':'pwned";
        var identity = new ClaimsIdentity([new("name", claimAttack)], "scheme");
        var request = new MockRequest("{'@mt':'Message'}");

        var json = await Handle(request, new(identity));

        Assert.Null(json["Injected"]);
        var claims = (JObject)json["Claims"]!;
        Assert.Equal(claimAttack, (string?)claims["name"]);
    }
}
