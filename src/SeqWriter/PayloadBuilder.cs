using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace SeqWriter
{
    public class PayloadBuilder
    {
        public PayloadBuilder(string appName)
        {
            application = new JProperty("Application", appName);
        }

        JToken version = new JProperty("ApplicationVersion", Assembly.GetEntryAssembly().GetName().Version.ToString());
        JToken machine = new JProperty("Machine", Environment.MachineName);
        JToken application;

        public IEnumerable<LogEvent> Build(HttpRequest request, ClaimsPrincipal user)
        {
            var logEvents = PayloadSerializer.GetLogEvents(request.Body);
            var userAgent = GetUserAgent(request);
            foreach (var logEvent in logEvents)
            {
                AppendExtraContext(logEvent, userAgent,user);
                yield return logEvent;
            }
        }

        static string GetUserAgent(HttpRequest request)
        {
            if (request.Headers.TryGetValue("User-Agent", out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        void AppendExtraContext(LogEvent logEvent, string userAgent, ClaimsPrincipal user)
        {
            if (string.IsNullOrWhiteSpace(logEvent.Timestamp))
            {
                throw new Exception("Missing timestamp");
            }

            //ToDictionary on Type is only safe in our case since we are using JWT tokens
            var dictionary = user.Claims
                .ToDictionary(x => x.Type.Split('/').Last(), x => x.Value);
            logEvent.Properties["Claims"] = JObject.FromObject(dictionary);
            if (userAgent != null)
            {
                logEvent.Properties.Add(new JProperty("UserAgent", userAgent));
            }

            logEvent.Properties.Add(version);
            logEvent.Properties.Add(machine);
            logEvent.Properties.Add(application);
        }
    }
}