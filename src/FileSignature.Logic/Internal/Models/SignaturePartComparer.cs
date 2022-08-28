namespace FileSignature.Logic.Internal.Models
{
    public class SignaturePartComparer : IComparer<SignaturePart>
    {
        public int Compare(SignaturePart x, SignaturePart y)
        {
            var result = x.PartNumber - y.PartNumber;

            if (result < 0)
                return -1;

            if (result > 0)
                return 1;

            return 0;
        }
    }
}
