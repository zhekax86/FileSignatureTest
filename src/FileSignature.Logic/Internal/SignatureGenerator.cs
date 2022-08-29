using FileSignature.Logic.Internal.Threads;
using FileSignature.Logic.Internal.Models;

namespace FileSignature.Logic.Internal
{
    public class SignatureGenerator
    {
        private readonly string _fileName;
        private readonly int _blockSize;

        private SignatureReaderState _state;

        private WaitHandle[] _waitHandles;
                
        public SignatureGenerator(string fileName, int blockSize)
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

                using (_state = Initialize(fileStream))
                {
                    StartThreads();
                                        
                    long currentBlock = 1L;
                    while (currentBlock <= _state.TotalBlocks)
                    {
                        yield return GetBlock(currentBlock);
                        currentBlock++;
                    }
                    
                    StopThreads();
                }
            }
        }

        public SignatureReaderState Initialize(FileStream fileStream)
        {
            var state = new SignatureReaderState();

            try 
            { 
                state.FileStream = fileStream;

                state.BlockSize = _blockSize;
                state.TotalBlocks = CalcBlocksCount(state.FileStream.Length);
                state.FileName = _fileName;

                state.CalculatingThreadsCount = GetCalculatingThreadsCount(Environment.ProcessorCount, state.TotalBlocks);
                state.MaxInputQueueLength = state.CalculatingThreadsCount * 4;

                state.BufferPool = new BufferPool(_blockSize);
                state.InputQueue = new Queue<InputQueueElement>(state.MaxInputQueueLength);
                state.OutputQueue = new SortedSet<SignaturePart>(new SignaturePartComparer());

                state.InputQueueSemaphore = new Semaphore(0, state.MaxInputQueueLength + 1);
                state.NextBlockNeededEvent = new AutoResetEvent(false);
                state.NewOutputElementEvent = new AutoResetEvent(false);
                state.StopThreadsEvent = new ManualResetEvent(false);

                state.BufferPoolLock = new SpinLock();
                state.InputQueueLock = new SpinLock();
                state.OutputQueueLock = new SpinLock();

                state.Errors = new List<Exception>();

                state.ReadingThread = new Thread(new ReadingThreadCode(state).Run);
                state.ReadingThread.Name = "File reading thread";
                state.ReadingThread.IsBackground = true;
            
                state.CalculatingThreads = new List<Thread>(state.CalculatingThreadsCount);
                for (int i = 0; i < state.CalculatingThreadsCount; i++)
                {
                    var calculatingThread = new Thread(new CalculatingThreadCode(state).Run);
                    calculatingThread.Name = $"Calculating thread #{i}";
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

        public SignaturePart GetBlock(long blockNumber)
        {
            long minBlockNumber;

            while (true)
            {
                var lockTaken = false;
                
                try
                {
                    _state.OutputQueueLock.Enter(ref lockTaken);

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
                finally
                {
                    if (lockTaken)
                        _state.OutputQueueLock.Exit();
                }

                WaitForNewOutputElement();

                if (_state.ErrorFlag)
                    lock (_state.Errors)
                        throw new DiagnosticException(_state.Errors, _state);
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
