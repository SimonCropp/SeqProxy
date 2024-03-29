﻿public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    #region ConfigureServices

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvcCore(option => option.EnableEndpointRouting = false);
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