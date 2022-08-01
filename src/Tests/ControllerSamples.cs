﻿namespace SimpleController
{
    #region SimpleController
    public class SeqController :
        BaseSeqController
    {
        public SeqController(SeqWriter seqWriter) :
            base(seqWriter)
        {
        }
    }
    #endregion
}

namespace OverridePostController
{
    #region OverridePostController
    public class SeqController :
        BaseSeqController
    {
        [CustomExceptionFilter]
        public override Task Post() =>
            base.Post();

        #endregion
        public SeqController(SeqWriter seqWriter) :
            base(seqWriter)
        {
        }
    }

    public class CustomExceptionFilterAttribute :
        ExceptionFilterAttribute
    {
    }
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