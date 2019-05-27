﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using SeqProxy;

namespace SimpleController
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
        public SeqController(SeqWriter seqWriter) :
            base(seqWriter)
        {
        }
        [CustomExceptionFilter]
        public override Task Post()
        {
            return base.Post();
        }
    }

    #endregion
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
    }
}

namespace AuthorizeController
{
    #region AuthorizeController
    [Authorize]
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