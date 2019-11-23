using System;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class PrefixBuilderTests :
    VerifyBase
{
    [Fact]
    public Task Build()
    {
        var builder = new PrefixBuilder("TheAppName", new Version(1, 2), s => s);
        var build = builder.Build(ClaimsBuilder.Build(), "theUserAgent", "theReferer");
        return Verify(build);
    }

    public PrefixBuilderTests(ITestOutputHelper output) :
        base(output)
    {
    }
}