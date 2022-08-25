using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic.Internal
{
    public class SignatureReader
    {
        private readonly string _fileName;
        private readonly int _blockSize;
        private SignatureReaderState _state;
                
        public SignatureReader(string fileName, int blockSize)
        {
            _fileName = fileName;
            _blockSize = blockSize;
        }


        public IEnumerable<SignaturePart> Read()
        {
            using(var state = new SignatureReaderState())
            {
                state.fileStream = File.OpenRead(_fileName);

                state.totalBlocks = CalcTotalBlocks(state.fileStream.Length);
                var calculatingThreadsCount = GetCalculatingThreadsCount(state.totalBlocks);
                
                state.readingThread = new Thread(ReadingThreadCode);
                state.readingThread.Start();

                state.calculatingThreads = new List<Thread>(calculatingThreadsCount);
                for (int i = 0; i < calculatingThreadsCount; i++)
                {
                    var calculatingThread = new Thread(CalculatingThreadCode);
                    calculatingThread.Start();

                    state.calculatingThreads.Add(calculatingThread);
                }
                
            }

            yield return new SignaturePart();
        }

        public long CalcTotalBlocks(long fileSize)
        {
            var result = fileSize / _blockSize;
            if (fileSize % _blockSize != 0)
                result++;

            return result;
        }

        public static int GetCalculatingThreadsCount(long blocksCount)
        {
            var processorCount = Environment.ProcessorCount;

            if (blocksCount > (long)processorCount)
                return processorCount;

            return (int)blocksCount;

        }

        public void ReadingThreadCode()
        {
            
        }

        public void CalculatingThreadCode()
        {

        }
    }
}
