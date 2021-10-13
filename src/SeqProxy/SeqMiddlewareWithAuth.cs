using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SeqProxy;

class SeqMiddlewareWithAuth
{
    RequestDelegate next;
    SeqWriter seqWriter;
    IAuthorizationService authService;

    public SeqMiddlewareWithAuth(RequestDelegate next, SeqWriter seqWriter, IAuthorizationService? authService = null)
    {
        this.next = next;
        this.seqWriter = seqWriter;
        if (authService is null)
        {
            throw new("Expected IAuthorizationService to be configured.");
        }

        this.authService = authService;
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