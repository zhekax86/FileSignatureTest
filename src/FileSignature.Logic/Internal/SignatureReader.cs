using FileSignature.Logic.Internal.Threads;
using FileSignature.Logic.Internal.Models;

namespace FileSignature.Logic.Internal
{
    public class SignatureReader
    {
        private readonly string _fileName;
        private readonly int _blockSize;
        private SignatureReaderState _state;

        private WaitHandle[] _waitHandles;
                
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

                _state.BlockSize = _blockSize;
                _state.totalBlocks = CalcBlocksCount(_state.fileStream.Length);

                if (_state.totalBlocks == 0)
                    yield break;

                var calculatingThreadsCount = GetCalculatingThreadsCount(_state.totalBlocks);
                _state.MaxInputQueueLength = calculatingThreadsCount * 2;

                _state.inputQueue = new Queue<InputQueueElement>(calculatingThreadsCount);
                _state.outputQueue = new SortedSet<SignaturePart>(new SignaturePartComparer());

                _state.inputQueueSemaphore = new Semaphore(0, int.MaxValue);
                _state.nextBlockNeededEvent = new AutoResetEvent(false);
                _state.newOutputElementEvent = new AutoResetEvent(false);
                _state.stopThreadsEvent = new ManualResetEvent(false);

                _state.Errors = new List<Exception>();

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

                _state.stopThreadsFlag = true;
                _state.stopThreadsEvent.Set();

                _state.readingThread.Join();
                foreach(var thread in _state.calculatingThreads)
                    thread.Join();


            }
        }

        public IEnumerable<SignaturePart> ReadOutputQueue()
        {
            long currentBlock = 1L;

            while (currentBlock <= _state.totalBlocks)
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

                WaitForNewOutputElement();

                if (_state.errorFlag)
                    lock (_state.Errors)
                        throw new AggregateException(_state.Errors);
            }
        }

        private void WaitForNewOutputElement()
        {
            _waitHandles ??= new WaitHandle[]
            {
                _state.stopThreadsEvent,
                _state.newOutputElementEvent
            };

            WaitHandle.WaitAny(_waitHandles);
        }

        public long CalcBlocksCount(long fileSize)
        {
            var result = fileSize / _blockSize;
            if (fileSize % _blockSize != 0)
                result++;

            return result;
        }

        public static int GetCalculatingThreadsCount(long blocksCount)
        {
            var processorCount = Environment.ProcessorCount;

            if (blocksCount > processorCount)
                return processorCount;

            return (int)blocksCount;
        }
    }
}
