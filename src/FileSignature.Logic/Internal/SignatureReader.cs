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
            using (var fileStream = File.OpenRead(_fileName))
            { 
                if (fileStream.Length == 0)
                    yield break;

                using (_state = InitializeInfrastructure(fileStream))
                {
                    StartThreads();

                    foreach (var val in ReadOutputQueue())
                        yield return val;

                    StopThreads();
                }
            }
        }

        public SignatureReaderState InitializeInfrastructure(FileStream fileStream)
        {
            var state = new SignatureReaderState();

            try 
            { 
                state.FileStream = fileStream;

                state.BlockSize = _blockSize;
                state.TotalBlocks = CalcBlocksCount(state.FileStream.Length);

                var calculatingThreadsCount = GetCalculatingThreadsCount(Environment.ProcessorCount, state.TotalBlocks);
                state.MaxInputQueueLength = calculatingThreadsCount * 2;

                state.InputQueue = new Queue<InputQueueElement>(state.MaxInputQueueLength);
                state.OutputQueue = new SortedSet<SignaturePart>(new SignaturePartComparer());

                state.InputQueueSemaphore = new Semaphore(0, state.MaxInputQueueLength);
                state.NextBlockNeededEvent = new AutoResetEvent(false);
                state.NewOutputElementEvent = new AutoResetEvent(false);
                state.StopThreadsEvent = new ManualResetEvent(false);

                state.Errors = new List<Exception>();

                state.ReadingThread = new Thread(new ReadingThreadCode(state).Run);
                state.ReadingThread.Name = "File reading thread";
                state.ReadingThread.IsBackground = true;
            
                state.CalculatingThreads = new List<Thread>(calculatingThreadsCount);
                for (int i = 0; i < calculatingThreadsCount; i++)
                {
                    var calculatingThread = new Thread(new CalculatingThreadCode(state).Run);
                    calculatingThread.Name = $"Calculationg thread #{i}";
                    calculatingThread.IsBackground = true;
                
                    state.CalculatingThreads.Add(calculatingThread);
                }

                return state;
            }
            catch
            {
                state.Dispose();
                throw;
            }
        }

        public void StartThreads()
        {
            _state.ReadingThread.Start();

            foreach (var thread in _state.CalculatingThreads)
                thread.Start();
        }

        public void StopThreads()
        {
            _state.StopThreadsFlag = true;
            _state.StopThreadsEvent.Set();

            _state.ReadingThread.Join();
            foreach (var thread in _state.CalculatingThreads)
                thread.Join();
        }

        public IEnumerable<SignaturePart> ReadOutputQueue()
        {
            long currentBlock = 1L;

            while (currentBlock <= _state.TotalBlocks)
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

                lock (_state.OutputQueue)
                {
                    var minBlock = _state.OutputQueue.Min;

                    if (minBlock == null)
                        minBlockNumber = -1L;
                    else
                    {
                        minBlockNumber = minBlock.PartNumber;
                        if (minBlockNumber == blockNumber)
                        {
                            _state.OutputQueue.Remove(minBlock);
                            return minBlock;
                        }
                    }
                }

                WaitForNewOutputElement();

                if (_state.ErrorFlag)
                    lock (_state.Errors)
                        throw new AggregateException(_state.Errors);
            }
        }

        private void WaitForNewOutputElement()
        {
            _waitHandles ??= new WaitHandle[]
            {
                _state.StopThreadsEvent,
                _state.NewOutputElementEvent
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

        public static int GetCalculatingThreadsCount(int processorCount, long blocksCount)
        {
            if (blocksCount > processorCount)
                return processorCount;

            return (int)blocksCount;
        }
    }
}
