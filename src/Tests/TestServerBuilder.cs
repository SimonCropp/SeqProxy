#pragma warning disable ASPDEPR008
#pragma warning disable ASPDEPR004

static class TestServerBuilder
{
    public static TestServer Build()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new(hostBuilder);
    }
}