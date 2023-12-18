[Route("/seqcontroller")]
public class SeqController(SeqWriter writer) :
    BaseSeqController(writer);