[UsesVerify]
public class PrefixBuilderTests
{
    [Fact]
    public Task Build()
    {
        var builder = new PrefixBuilder("TheAppName", new(1, 2), s => s, "theMachine", "theUser");
        var build = builder.Build(ClaimsBuilder.Build(), "theUserAgent", "theReferer", "theId");
        return Verify(build);
    }
}