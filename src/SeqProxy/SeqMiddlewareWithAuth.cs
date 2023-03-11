class SeqMiddlewareWithAuth
{
    RequestDelegate next;
    SeqWriter seqWriter;
    IAuthorizationService authService;

    public SeqMiddlewareWithAuth(RequestDelegate next, SeqWriter seqWriter, IAuthorizationService? authService = null)
    {
        this.next = next;
        this.seqWriter = seqWriter;

        this.authService = authService ?? throw new("Expected IAuthorizationService to be configured.");
    }

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

        await seqWriter.Handle(
            user,
            context.Request,
            context.Response,
            context.RequestAborted);
    }

    #endregion
}