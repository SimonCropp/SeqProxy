[Route("/seqcontroller")]
public class SeqController :
    BaseSeqController
{
    public SeqController(SeqWriter seqWriter) :
        base(seqWriter)
    {
    }
}