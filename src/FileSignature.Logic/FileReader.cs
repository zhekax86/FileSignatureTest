using FileSignature.Logic.Internal;

namespace FileSignature.Logic
{
    public static class FileReader
    {
        public static IEnumerable<SignaturePart> GetSignature(string fileName, int blockSize)
        {
            var reader = new SignatureGenerator(fileName, blockSize);
            return reader.Read();
        }
    }
}