namespace FileSignature.Logic.Internal.Models
{
    public class InputQueueElement
    {
        public long BlockNumber { get; set; }
        public byte[] Buffer { get; set; }
        public int BufferLength { get; set; }
    }
}
