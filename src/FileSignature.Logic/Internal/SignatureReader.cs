using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            using(_state = new SignatureReaderState())
            {
                _state.fileStream = File.OpenRead(_fileName);

                _state.totalBlocks = CalcTotalBlocks(_state.fileStream.Length);
                var calculatingThreadsCount = GetCalculatingThreadsCount(_state.totalBlocks);

                _state.inputQueue = new Queue<InputQueueElement>(calculatingThreadsCount);
                _state.outputQueue = new SortedSet<SignaturePart>();

                _state.inputQueueSemaphore = new Semaphore(0, int.MaxValue);
                _state.nextBlockNeededEvent = new AutoResetEvent(false);
                _state.newOutputElementEvent = new AutoResetEvent(false);

                _state.readingThread = new Thread(new ReadingThreadCode(_state).Run);
                _state.readingThread.Start();

                _state.calculatingThreads = new List<Thread>(calculatingThreadsCount);
                for (int i = 0; i < calculatingThreadsCount; i++)
                {
                    var calculatingThread = new Thread(new CalculatingThreadCode(_state).Run);
                    calculatingThread.Start();

                    _state.calculatingThreads.Add(calculatingThread);
                }

                foreach(var val in ReadOutputQueue())
                    yield return val;

                
            }
        }

        public IEnumerable<SignaturePart> ReadOutputQueue()
        {
            long currentBlock = 0L;

            while (currentBlock < _state.totalBlocks)
            {
                yield return GetBlock(currentBlock);
                currentBlock++;
            }
        }

        public SignaturePart GetBlock(long blockNumber)
        {
            long minBlockNumber;

            while (true)
            {

                lock (_state.outputQueue)
                {
                    var minBlock = _state.outputQueue.Min;

                    if (minBlock == null)
                        minBlockNumber = -1L;
                    else
                    {
                        minBlockNumber = minBlock.PartNumber;
                        if (minBlockNumber == blockNumber)
                        {
                            _state.outputQueue.Remove(minBlock);
                            return minBlock;
                        }
                    }
                }

                _state.newOutputElementEvent.WaitOne();
            }
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
    }
}
