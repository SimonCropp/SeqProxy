﻿#pragma warning disable CA1822
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
            scrubClaimType: claimType =>
            {
                var lastIndexOf = claimType.LastIndexOf('/');
                if (lastIndexOf == -1)
                {
                    return claimType;
                }

                return claimType[(lastIndexOf + 1)..];
            });
    }

    #endregion
}