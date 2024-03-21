using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

class Content : HttpContent
{
    string toString;
    static MediaTypeHeaderValue contentType = new("application/vnd.serilog.clef", Encoding.UTF8.WebName);

    public Content(string toString)
    {
        this.toString = toString;
        Headers.ContentType = contentType;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var writer = new StreamWriter(stream);
        writer.Write(toString);
        writer.Flush();
        return Task.CompletedTask;
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}