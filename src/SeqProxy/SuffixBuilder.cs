using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SeqWriter
{
    public class SuffixBuilder
    {
        string suffix;
        public SuffixBuilder(string appName, Version version)
        {
            var suffix = new Suffix
            {
                AppName= appName,
                AppVersion = version.ToString(),
                Server = Environment.MachineName,
            };
            var serialized = suffix.ToJson();
            this.suffix = ", "+ serialized.Substring(1,serialized.Length-2);
        }

        public string Build(HttpRequest request, ClaimsPrincipal user)
        {
            var userAgent = request.GetUserAgent();
            return Build(user, userAgent);
        }

        public string Build(ClaimsPrincipal user, string userAgent)
        {
            var builder = new StringBuilder(suffix);
            if (user.Claims.Any())
            {
                var claims = user.Claims.ToDictionary(x => x.Type, x => x.Value);
                builder.Append($",'Claims':{JObject.FromObject(claims)}");
            }

            builder.Append($",'UserAgent':{JsonConvert.SerializeObject(userAgent)}");
            return builder.ToString();
        }
    }
}