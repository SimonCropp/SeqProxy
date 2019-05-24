using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public static class PayloadSerializer
{
    static JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings {DateParseHandling = DateParseHandling.None});

    public static string Serialize(List<LogEvent> logEvents)
    {
        var stringBuilder = new StringBuilder();
        using (var stringWriter = new StringWriter(stringBuilder))
        using (var jsonTextWriter = new JsonTextWriter(stringWriter))
        {
            serializer.Serialize(jsonTextWriter, new RootObject {Events = logEvents});
        }

        return stringBuilder.ToString();
    }

    public static List<LogEvent> GetLogEvents(Stream stream)
    {
        using (var streamReader = new StreamReader(stream))
        using (var textReader = new JsonTextReader(streamReader))
        {
            return serializer.Deserialize<RootObject>(textReader).Events;
        }
    }
}