using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SeqProxy
{
    [Route("/api/events/raw")]
    [Route("/seq")]
    [ApiController]
    public abstract class BaseSeqController :
        ControllerBase
    {
        SeqWriter seqWriter;

        protected BaseSeqController(SeqWriter seqWriter)
        {
            Guard.AgainstNull(seqWriter, nameof(seqWriter));
            this.seqWriter = seqWriter;
        }

        [HttpPost]
        public virtual Task Post()
        {
            return seqWriter.Handle(User, Request, Response,HttpContext.RequestAborted );
        }
    }
}