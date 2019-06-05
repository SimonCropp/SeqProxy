using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SeqProxy;

public class SeqMiddleware
{
    RequestDelegate next;
    SeqWriter seqWriter;

    public SeqMiddleware(RequestDelegate next, SeqWriter seqWriter)
    {
        this.next = next;
        this.seqWriter = seqWriter;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/api/events/raw")
        {
            return seqWriter.Handle(context.User, context.Request, context.Response, context.RequestAborted);
        }

        return next(context);
    }
}