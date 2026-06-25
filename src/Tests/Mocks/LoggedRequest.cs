public class LoggedRequest
{
    public LoggedRequest(string body) =>
        Body = body;

    public readonly string Body;
    public long? ContentLength;
    public Uri? Uri;
    public string? ApiKey;
}