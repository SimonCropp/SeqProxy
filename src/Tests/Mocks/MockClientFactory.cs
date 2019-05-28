using System.Net.Http;

public class MockClientFactory : IHttpClientFactory
{
    public MockHttpClient Client = new MockHttpClient();

    public HttpClient CreateClient(string name)
    {
        return Client;
    }
}