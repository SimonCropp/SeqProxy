using System;
using System.Collections.Generic;
using System.Security.Claims;
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
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("claimType", "superadmin")
                },
                "Basic"));
        var build = builder.Build(user, "theUserAgent");
        Approvals.Verify(build,s => s.Replace(Environment.MachineName,"TheMachineName"));
    }

    public PrefixBuilderTests(ITestOutputHelper output) :
        base(output)
    {
    }
}