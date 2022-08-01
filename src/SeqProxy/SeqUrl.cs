using Microsoft.AspNetCore.Http;

static class SeqUrl
{
    public static bool IsSeqUrl(this HttpContext context) =>
        context.Request.Path == "/api/events/raw" ||
        context.Request.Path == "/seq";
}