using Microsoft.AspNetCore.Http;

// Non-nullable field is uninitialized.
#pragma warning disable CS8618
public class MockRequest :
    HttpRequest
{
    public MockRequest(string body)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(body);
        writer.Flush();
        stream.Position = 0;
        Body = stream;
    }

    public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public override HttpContext HttpContext { get; }
    public override string Method { get; set; }
    public override string Scheme { get; set; }
    public override bool IsHttps { get; set; }
    public override HostString Host { get; set; }
    public override PathString PathBase { get; set; }
    public override PathString Path { get; set; }
    public override QueryString QueryString { get; set; }
    public override IQueryCollection Query { get; set; } = new QueryCollection();
    public override string Protocol { get; set; }
    public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
    public override IRequestCookieCollection Cookies { get; set; } = new MockCookieCollection();
    public override long? ContentLength { get; set; }
    public override string? ContentType { get; set; }
    public override Stream Body { get; set; }
    public override bool HasFormContentType { get; }
    public override IFormCollection Form { get; set; } = new FormCollection(new());
}