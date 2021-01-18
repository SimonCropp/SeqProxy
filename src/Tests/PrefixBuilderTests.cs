using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class PrefixBuilderTests
{
    [Fact]
    public Task Build()
    {
        PrefixBuilder builder = new("TheAppName", new(1, 2), s => s);
        var build = builder.Build(ClaimsBuilder.Build(), "theUserAgent", "theReferer", "theId");
        return Verifier.Verify(build);
    }
}