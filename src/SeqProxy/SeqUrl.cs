using Microsoft.AspNetCore.Http;

static class SeqUrl
{
    public  static bool IsSeqUrl(this HttpContext context)
    {
        return context.Request.Path == "/api/events/raw" ||
               context.Request.Path == "/seq";
    }
}