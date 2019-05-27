using System.IO;
using System.Text;
using Newtonsoft.Json;

static class Serializer
{
    public static string ToJson(this object target)
    {
        var jsonSerializer = JsonSerializer.Create();
        var builder = new StringBuilder();
        using (var stringWriter = new StringWriter(builder))
        using (var writer = new JsonTextWriter(stringWriter))
        {
            writer.Formatting = Formatting.None;
            writer.QuoteChar = '\'';
            jsonSerializer.Serialize(writer,target);
        }

        return builder.ToString();
    }
}