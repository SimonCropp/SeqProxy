class SeqMiddlewareWithAuth(RequestDelegate next, SeqWriter writer, IAuthorizationService? authService = null)
{
    IAuthorizationService authService = authService ?? throw new("Expected IAuthorizationService to be configured.");

    public Task InvokeAsync(HttpContext context)
    {
        if (!context.IsSeqUrl())
        {
            return next(context);
        }

        return HandleWithAuth(context);
    }

    #region HandleWithAuth

    async Task HandleWithAuth(HttpContext context)
    {
        var user = context.User;
        var authResult = await authService.AuthorizeAsync(user, null, "SeqLog");

        if (!authResult.Succeeded)
        {
            await context.ChallengeAsync();
            return;
        }

        await writer.Handle(
            user,
            context.Request,
            context.Response,
            context.RequestAborted);
    }

    #endregion
}