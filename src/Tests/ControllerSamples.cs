namespace SimpleController
{
    #region SimpleController
    public class SeqController(SeqWriter seqWriter) :
        BaseSeqController(seqWriter);
    #endregion
}

namespace OverridePostController
{
    #region OverridePostController
    public class SeqController(SeqWriter seqWriter) :
        BaseSeqController(seqWriter)
    {
        [CustomExceptionFilter]
        public override Task Post() =>
            base.Post();

        #endregion
    }

    public class CustomExceptionFilterAttribute :
        ExceptionFilterAttribute;
}

namespace AuthorizeController
{
    #region AuthorizeController
    [Authorize]
    public class SeqController :
        BaseSeqController
    #endregion
    {
        public SeqController(SeqWriter seqWriter) :
            base(seqWriter)
        {
        }
    }
}