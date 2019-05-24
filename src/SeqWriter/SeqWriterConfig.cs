using SeqWriter;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SeqWriterConfig
    {
        public static void AddSeqWriter(this IServiceCollection services, string seqUrl, string appName, string apikey = null)
        {
            Guard.AgainstEmpty(apikey, nameof(apikey));
            Guard.AgainstNullOrEmpty(appName, nameof(appName));
            Guard.AgainstNullOrEmpty(seqUrl, nameof(seqUrl));
            Guard.AgainstNull(services, nameof(services));
            services.AddSingleton(new Poster(seqUrl, apikey));
            services.AddSingleton(new PayloadBuilder(appName));
        }
    }
}