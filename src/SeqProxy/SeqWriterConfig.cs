using System;
using System.Net.Http;
using System.Reflection;
using SeqProxy;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SeqWriterConfig
    {
        public static void AddSeqWriter(
            this IServiceCollection services,
            string seqUrl,
            string appName = null,
            Version appVersion = null,
            string apiKey = null,
            Func<string,string> scrubClaimType = null)
        {
            Guard.AgainstEmpty(apiKey, nameof(apiKey));
            Guard.AgainstEmpty(appName, nameof(appName));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(services, nameof(services));
            services.AddHttpClient();
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
                    return new SeqWriter(httpFactory, seqUrl, appName, appVersion, apiKey, scrubClaimType);
                });
        }
    }
}