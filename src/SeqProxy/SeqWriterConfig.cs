using System;
using SeqProxy;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SeqWriterConfig
    {
        public static void AddSeqWriter(this IServiceCollection services, string seqUrl, string appName, Version appVersion, string apiKey = null, bool swallowSeqExceptions = false)
        {
            Guard.AgainstEmpty(apiKey, nameof(apiKey));
            Guard.AgainstNullOrEmpty(appName, nameof(appName));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(services, nameof(services));
            services.AddSingleton(new Poster(seqUrl, appName, appVersion, apiKey, swallowSeqExceptions));
        }
    }
}