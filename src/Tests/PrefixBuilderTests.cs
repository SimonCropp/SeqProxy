[UsesVerify]
public class PrefixBuilderTests
{
    [Fact]
    public Task Build()
    {
        PrefixBuilder builder = new("TheAppName", new(1, 2), s => s, "theMachine");
        var build = builder.Build(ClaimsBuilder.Build(), "theUserAgent", "theReferer", "theId");
        return Verify(build);
    }
}