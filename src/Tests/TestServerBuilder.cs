using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

static class TestServerBuilder
{
    public static TestServer Build()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }
}