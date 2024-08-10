using System.Security.Claims;

static class ClaimsBuilder
{
    public static ClaimsPrincipal Build()
    {
        var principal = new ClaimsPrincipal();
        principal.AddIdentity(
            new(
                [
                    new(ClaimTypes.Email, "User@foo.bar"),
                    new(ClaimTypes.UserData, "theUserData")
                ],
                "FakeScheme"));
        return principal;
    }
}