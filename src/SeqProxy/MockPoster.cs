using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SeqProxy
{
    public class MockPoster :
        Poster
    {
        public List<LogEntry> Entries { get; }

        public MockPoster()
        {
            Entries = new List<LogEntry>();
        }

        public class LogEntry
        {
            public ClaimsPrincipal User { get; set; }
            public string Payload { get; set; }
        }

        public override async Task Handle(ClaimsPrincipal user, HttpRequest request, HttpResponse response)
        {
            using (var streamReader = new StreamReader(request.Body))
            {
                Entries.Add(
                    new LogEntry
                    {
                        User = user,
                        Payload = await streamReader.ReadToEndAsync()
                    });
            }
        }
    }
}