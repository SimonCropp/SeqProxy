using System;
using ApprovalTests;
using Xunit;
using Xunit.Abstractions;

public class PrefixBuilderTests :
    XunitApprovalBase
{
    [Fact]
    public void Build()
    {
        var builder = new PrefixBuilder("TheAppName", new Version(1, 2), s => s);
        var build = builder.Build(ClaimsBuilder.Build(), "theUserAgent", "theReferer");
        Approvals.Verify(build, Scrubber.Scrub);
    }

    public PrefixBuilderTests(ITestOutputHelper output) :
        base(output)
    {
    }
}