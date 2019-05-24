using Newtonsoft.Json.Linq;

public class LogEvent
{
    public string Timestamp;
    public string MessageTemplate;
    public JObject Properties;
    public string Level;
    public string Exception;
}