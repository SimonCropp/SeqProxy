using System;
using System.Threading.Tasks;
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
        if (authService == null)
        {
            throw new Exception("Expected IAuthorizationService to be configured.");
        }

        this.authService = authService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.IsSeqUrl())
        {
            await next(context);
            return;
        }

        await HandleWithAuth(context, authService);
    }

    #region HandleWithAuth

    async Task HandleWithAuth(HttpContext context, IAuthorizationService authService)
    {
        var authResult = await authService.AuthorizeAsync(context.User, null, "SeqLog");

        if (!authResult.Succeeded)
        {
            await context.ChallengeAsync();
            return;
        }

        await seqWriter.Handle(context.User, context.Request, context.Response, context.RequestAborted);
    }

    #endregion
}