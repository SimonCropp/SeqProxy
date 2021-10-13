using Microsoft.Extensions.DependencyInjection;

public class StartupSnippets
{
    #region ConfigureServicesFull

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvcCore();
        services.AddSeqWriter(
            seqUrl: "http://localhost:5341",
            apiKey: "TheApiKey",
            application: "MyAppName",
            appVersion: new(1, 2),
            scrubClaimType: claimType => claimType.Split("/").Last());
    }

    #endregion
}