class SeqMiddleware
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
        if (!context.IsSeqUrl())
        {
            return next(context);
        }

        return seqWriter.Handle(context.User, context.Request, context.Response, context.RequestAborted);
    }
}