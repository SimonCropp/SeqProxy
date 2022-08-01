using System.Security.Claims;

static class ClaimsBuilder
{
    public static ClaimsPrincipal Build()
    {
        var principal = new ClaimsPrincipal();
        principal.AddIdentity(
            new(
                new[]
                {
                    new Claim(ClaimTypes.Email, "User@foo.bar"),
                    new Claim(ClaimTypes.UserData, "theUserData")
                },
                "FakeScheme"));
        return principal;
    }
}