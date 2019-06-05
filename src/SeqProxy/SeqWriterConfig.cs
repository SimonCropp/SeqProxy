using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using SeqProxy;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SeqWriterConfig
    {
        public static void UseSeq(this IApplicationBuilder builder, bool useAuthorizationService = false)
        {
            if (useAuthorizationService)
            {
                builder.UseMiddleware<SeqMiddlewareWithAuth>();
                return;
            }
            builder.UseMiddleware<SeqMiddleware>();
        }

        public static void AddSeqWriter(
            this IServiceCollection services,
            string seqUrl,
            string appName = null,
            Version appVersion = null,
            string apiKey = null,
            Func<string, string> scrubClaimType = null,
            Action<IServiceProvider, HttpClient> configureClient = null)
        {
            Guard.AgainstEmpty(apiKey, nameof(apiKey));
            Guard.AgainstEmpty(appName, nameof(appName));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(services, nameof(services));

            AddHttpClient(services, configureClient);

            if (scrubClaimType == null)
            {
                scrubClaimType = DefaultClaimTypeScrubber.Scrub;
            }

            if (appName == null || appVersion == null)
            {
                var callingAssemblyName = Assembly.GetCallingAssembly().GetName();
                if (appName == null)
                {
                    appName = callingAssemblyName.Name;
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
                    return new SeqWriter(() => httpFactory.CreateClient("SeqProxy"), seqUrl, appName, appVersion, apiKey, scrubClaimType);
                });
        }

        static void AddHttpClient(IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
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