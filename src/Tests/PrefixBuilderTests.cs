using System;
using ApprovalTests;
using Xunit;
using Xunit.Abstractions;

public class PrefixBuilderTests :
    XunitLoggingBase
{
    [Fact]
    public void Build()
    {
        var builder = new PrefixBuilder("TheAppName", new Version(1, 2));
        var build = builder.Build(ClaimsBuilder.Build(), "theUserAgent");
        Approvals.Verify(build,s => s.Replace(Environment.MachineName,"TheMachineName"));
    }

    public PrefixBuilderTests(ITestOutputHelper output) :
        base(output)
    {
    }
}