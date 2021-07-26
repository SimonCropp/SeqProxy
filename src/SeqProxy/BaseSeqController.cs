using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SeqProxy
{
    /// <summary>
    /// An implementation of <see cref="ControllerBase"/> that provides a http post and some basic routing.
    /// </summary>
    [Route("/api/events/raw")]
    [Route("/seq")]
    [ApiController]
    public abstract class BaseSeqController :
        ControllerBase
    {
        SeqWriter seqWriter;

        /// <summary>
        /// Initializes a new instance of <see cref="BaseSeqController"/>
        /// </summary>
        protected BaseSeqController(SeqWriter seqWriter)
        {
            this.seqWriter = seqWriter;
        }

        /// <summary>
        /// Handles log events via a HTTP post.
        /// </summary>
        [HttpPost]
        public virtual Task Post()
        {
            return seqWriter.Handle(User, Request, Response, HttpContext.RequestAborted);
        }
    }
}