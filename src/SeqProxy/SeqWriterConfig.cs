using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using SeqProxy;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds
    /// </summary>
    public static class SeqWriterConfig
    {
        /// <summary>
        /// Adds a Middleware to <paramref name="builder"/> that handles log requests and forwards them to SEQ.
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
        /// <param name="seqUrl">The SEQ api url.</param>
        /// <param name="application">The application name. If null then the name of <see cref="Assembly.GetCallingAssembly"/> will be used.</param>
        /// <param name="appVersion">The application version. If null then the version of <see cref="Assembly.GetCallingAssembly"/> will be used.</param>
        /// <param name="apiKey">The SEQ api key to use. Will be appended to <paramref name="seqUrl"/> when writing log entries.</param>
        /// <param name="scrubClaimType">Scrubber for claim types. If null then <see cref="DefaultClaimTypeScrubber.Scrub"/> will be used.</param>
        /// <param name="configureClient">Call back for manipulating the <see cref="HttpClient"/> use for writing log entries to SEQ.</param>
        public static void AddSeqWriter(
            this IServiceCollection services,
            string seqUrl,
            string? application = null,
            Version? appVersion = null,
            string? apiKey = null,
            Func<string, string>? scrubClaimType = null,
            Action<IServiceProvider, HttpClient>? configureClient = null)
        {
            Guard.AgainstEmpty(apiKey, nameof(apiKey));
            Guard.AgainstEmpty(application, nameof(application));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(services, nameof(services));

            AddHttpClient(services, configureClient);

            if (scrubClaimType == null)
            {
                scrubClaimType = DefaultClaimTypeScrubber.Scrub;
            }

            if (application == null || appVersion == null)
            {
                var callingAssemblyName = Assembly.GetCallingAssembly().GetName();
                if (application == null)
                {
                    application = callingAssemblyName.Name;
                }

                if (appVersion == null)
                {
                    appVersion = callingAssemblyName.Version;
                }
            }

            services.AddSingleton(
                provider =>
                {
                    var httpFactory = provider.GetService<IHttpClientFactory>();
                    return new SeqWriter(
                        httpClientFunc: () => httpFactory.CreateClient("SeqProxy"),
                        seqUrl!,
                        application,
                        appVersion,
                        apiKey,
                        scrubClaimType);
                });
        }

        static void AddHttpClient(IServiceCollection services, Action<IServiceProvider, HttpClient>? configureClient)
        {
            if (configureClient == null)
            {
                services.AddHttpClient("SeqProxy");
            }
            else
            {
                services.AddHttpClient("SeqProxy", configureClient);
            }
        }
    }
}