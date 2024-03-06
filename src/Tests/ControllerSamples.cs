namespace SimpleController
{
    #region SimpleController
    public class SeqController(SeqWriter writer) :
        BaseSeqController(writer);
    #endregion
}

namespace OverridePostController
{
    #region OverridePostController
    public class SeqController(SeqWriter writer) :
        BaseSeqController(writer)
    {
        [CustomExceptionFilter]
        public override Task Post() =>
            base.Post();

        #endregion
    }

    public sealed class CustomExceptionFilterAttribute :
        ExceptionFilterAttribute;
}

namespace AuthorizeController
{
    #region AuthorizeController
    [Authorize]
    public class SeqController(SeqWriter writer) :
        BaseSeqController(writer)

    #endregion

    {
    }
}