using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SeqProxy;

public class SeqMiddlewareWithAuth
{
    RequestDelegate next;
    SeqWriter seqWriter;
    IAuthorizationService authorizationService;

    public SeqMiddlewareWithAuth(RequestDelegate next, SeqWriter seqWriter, IAuthorizationService authorizationService)
    {
        this.next = next;
        this.seqWriter = seqWriter;
        this.authorizationService = authorizationService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path != "/api/events/raw")
        {
            await next(context);
            return;
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(context.User, null, "null");

        if (!authorizationResult.Succeeded)
        {
            await context.ChallengeAsync();
            return;
        }

        await seqWriter.Handle(context.User, context.Request, context.Response, context.RequestAborted);
    }
}