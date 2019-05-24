using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SeqWriter
{
    [Route("/api/events/raw")]
    [ApiController]
    public abstract class BaseSeqController :
        ControllerBase
    {
        PayloadBuilder payloadBuilder;
        Poster poster;

        public BaseSeqController(PayloadBuilder payloadBuilder, Poster poster)
        {
            this.payloadBuilder = payloadBuilder;
            this.poster = poster;
        }

        [HttpPost]
        public Task Post()
        {
            var logEvents = payloadBuilder.Build(Request,User).ToList();
            var payload = PayloadSerializer.Serialize(logEvents);
            return poster.Write(payload, Response);
        }
    }
}