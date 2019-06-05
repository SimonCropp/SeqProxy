using Microsoft.AspNetCore.Mvc;
using SeqProxy;

[Route("/seqcontroller")]
public class SeqController :
    BaseSeqController
{
    public SeqController(SeqWriter seqWriter) :
        base(seqWriter)
    {
    }
}