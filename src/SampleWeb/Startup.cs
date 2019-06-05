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

    #region ConfigureServices

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvcCore();
        services.AddSeqWriter(seqUrl: "http://localhost:5341");
    }

    #endregion

    #region ConfigureBuilder

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseSeq();
    #endregion
        builder.UseMvc();
    }
}