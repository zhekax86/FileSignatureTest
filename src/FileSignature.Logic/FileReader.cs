using FileSignature.Logic.Internal;

namespace FileSignature.Logic
{
    public class FileReader
    {
        public IEnumerable<SignaturePart> GetSignature(string fileName, int blockSize)
        {
            var reader = new SignatureReader(fileName, blockSize);
            return reader.Read();
        }
    }
}