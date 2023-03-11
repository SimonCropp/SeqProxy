namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Adds
/// </summary>
public static class SeqWriterConfig
{
    /// <summary>
    /// Adds a Middleware to <paramref name="builder"/> that handles log requests and forwards them to Seq.
    /// </summary>
    public static void UseSeq(this IApplicationBuilder builder, bool useAuthorizationService = false)
    {
        if (useAuthorizationService)
        {
            builder.UseMiddleware<SeqMiddlewareWithAuth>();
            return;
        }

        builder.UseMiddleware<SeqMiddleware>();
    }

    /// <summary>
    /// Adds a <see cref="SeqWriter"/> singleton to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to ad to.</param>
    /// <param name="seqUrl">The Seq api url.</param>
    /// <param name="application">The application name. If null then the name of <see cref="Assembly.GetCallingAssembly"/> will be used.</param>
    /// <param name="appVersion">The application version. If null then the version of <see cref="Assembly.GetCallingAssembly"/> will be used.</param>
    /// <param name="apiKey">The Seq api key to use. Will be appended to <paramref name="seqUrl"/> when writing log entries.</param>
    /// <param name="server">The value to use for the Seq `Server` property. Defaults to <see cref="Environment.MachineName"/>.</param>
    /// <param name="user">The value to use for the Seq `User` property. Defaults to <see cref="Environment.UserName"/>.</param>
    /// <param name="scrubClaimType">Scrubber for claim types. If null then <see cref="DefaultClaimTypeScrubber.Scrub"/> will be used.</param>
    /// <param name="configureClient">Call back for manipulating the <see cref="HttpClient"/> use for writing log entries to Seq.</param>
    public static void AddSeqWriter(
        this IServiceCollection services,
        string seqUrl,
        string? application = null,
        Version? appVersion = null,
        string? apiKey = null,
        string? server = null,
        string? user = null,
        Func<string, string>? scrubClaimType = null,
        Action<IServiceProvider, HttpClient>? configureClient = null)
    {
        Guard.AgainstEmpty(apiKey, nameof(apiKey));
        Guard.AgainstEmpty(server, nameof(server));
        Guard.AgainstEmpty(user, nameof(user));
        Guard.AgainstEmpty(application, nameof(application));
        Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));

        AddHttpClient(services, configureClient);

        scrubClaimType ??= DefaultClaimTypeScrubber.Scrub;

        if (application == null ||
            appVersion is null)
        {
            var callingAssemblyName = Assembly.GetCallingAssembly().GetName();
            application ??= callingAssemblyName.Name;
            appVersion ??= callingAssemblyName.Version;
        }

        server ??= Environment.MachineName;
        user ??= Environment.UserName;

        services.AddSingleton(
            provider =>
            {
                var httpFactory = provider.GetRequiredService<IHttpClientFactory>();
                return new SeqWriter(
                    httpClientFunc: () => httpFactory.CreateClient("SeqProxy"),
                    seqUrl: seqUrl,
                    application: application!,
                    version: appVersion!,
                    apiKey: apiKey,
                    scrubClaimType: scrubClaimType,
                    server: server,
                    user: user);
            });
    }

    static void AddHttpClient(IServiceCollection services, Action<IServiceProvider, HttpClient>? configureClient)
    {
        if (configureClient is null)
        {
            services.AddHttpClient("SeqProxy");
        }
        else
        {
            services.AddHttpClient("SeqProxy", configureClient);
        }
    }
}