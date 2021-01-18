using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

static class TestServerBuilder
{
    public static TestServer Build()
    {
        WebHostBuilder hostBuilder = new();
        hostBuilder.UseStartup<Startup>();
        return new(hostBuilder);
    }
}