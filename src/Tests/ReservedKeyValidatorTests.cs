// Regression tests for finding #2: a client must not be able to forge the proxy-stamped
// audit fields by supplying them in the request body. Only keys clients never legitimately
// send are reserved (SeqProxyId, Claims); generic names like User/Server stay allowed.
public class ReservedKeyValidatorTests
{
    [Theory]
    [InlineData("{'@mt':'x','SeqProxyId':'forged'}")]
    [InlineData("{'@mt':'x','Claims':{'role':'admin'}}")]
    [InlineData("{\"@mt\":\"x\",\"SeqProxyId\":\"forged\"}")]
    [InlineData("{\"@mt\":\"x\",\"Claims\":{}}")]
    public void ReservedKeysAreRejected(string line) =>
        Assert.Throws<Exception>(() => ReservedKeyValidator.ThrowIfReservedKey(line));

    [Theory]
    [InlineData("{'@mt':'Hello, {User}','User':'John'}")] // legitimate client property
    [InlineData("{'@mt':'connected to {Server}','Server':'db01'}")] // legitimate client property
    [InlineData("{'@mt':'x','Application':'ClientApp'}")]
    [InlineData("{'@mt':'just a message'}")]
    public void LegitimatePropertiesAreAllowed(string line) =>
        ReservedKeyValidator.ThrowIfReservedKey(line);

    [Fact]
    public async Task ForgedEventIsNotForwardedToSeq()
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
        var request = new MockRequest("{'@mt':'Message','SeqProxyId':'forged'}");

        await Assert.ThrowsAsync<Exception>(() => writer.Handle(new(), request, new MockResponse()));
        // Fail closed: nothing is forwarded to Seq.
        Assert.Empty(httpClient.Requests);
    }
}
