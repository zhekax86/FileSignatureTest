using FileSignature.Logic.Internal;

namespace FileSignature.Logic
{
    public static class FileReader
    {
        public static IEnumerable<SignaturePart> GetSignature(string fileName, int blockSize)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(blockSize));

            if (blockSize > MaxSignatureBlockSize)
                throw new ArgumentOutOfRangeException(nameof(blockSize));

            var generator = new SignatureGenerator(fileName, blockSize);
            return generator.Read();
        }

        public static int MaxSignatureBlockSize => 1024 * 1024;
    }
}