using FileSignature.Logic.Internal;

namespace FileSignature.Logic
{
    public static class FileReader
    {
        public static IEnumerable<SignaturePart> GetSignature(string fileName, int blockSize)
        {
            var reader = new SignatureReader(fileName, blockSize);
            return reader.Read();
        }
    }
}