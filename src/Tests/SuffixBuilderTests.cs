using System;
using System.Collections.Generic;
using System.Security.Claims;
using ApprovalTests;
using SeqWriter;
using Xunit;
using Xunit.Abstractions;

public class SuffixBuilderTests :
    XunitLoggingBase
{
    [Fact]
    public void Build()
    {
        var builder = new SuffixBuilder("TheAppName", new Version(1, 2));
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("claimType", "superadmin")
                },
                "Basic"));
        var build = builder.Build(user, "theUserAgent");
        Approvals.Verify(build);
    }

    public SuffixBuilderTests(ITestOutputHelper output) :
        base(output)
    {
    }
}