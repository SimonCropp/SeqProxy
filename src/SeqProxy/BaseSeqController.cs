using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SeqProxy
{
    [Route("/api/events/raw")]
    [ApiController]
    public abstract class BaseSeqController :
        ControllerBase
    {
        Poster poster;

        protected BaseSeqController(Poster poster)
        {
            this.poster = poster;
        }

        [HttpPost]
        public Task Post()
        {
            return poster.Handle(User,Request, Response);
        }
    }
}