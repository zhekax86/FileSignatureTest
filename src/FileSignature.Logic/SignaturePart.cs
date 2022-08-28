namespace FileSignature.Logic
{
    public class SignaturePart
    {
        public long PartNumber { get; set; }

        public long TotalParts { get; set; }

        public byte[] Hash { get; set; }
    }
}
