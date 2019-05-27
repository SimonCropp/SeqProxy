using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvcCore();
        services.AddSeqWriter(
            seqUrl: "http://localhost:5341",
            appName: "Sample",
            appVersion: GetType().Assembly.GetName().Version);
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseMvc();
    }
}