using Newtonsoft.Json;
using System.Text;

namespace FileSignature.Logic.Internal.Models
{
    public class SignatureReaderState : IDisposable
    {
        public int BlockSize;
        public long TotalBlocks;
        public string FileName;

        [JsonIgnore]
        public FileStream FileStream;
        [JsonIgnore]
        public Thread ReadingThread;
        [JsonIgnore]
        public List<Thread> CalculatingThreads;
        public int CalculatingThreadsCount;

        [JsonIgnore]
        public BufferPool BufferPool;
        [JsonIgnore]
        public volatile Queue<InputQueueElement> InputQueue;
        public int MaxInputQueueLength;
        [JsonIgnore]
        volatile public SortedSet<SignaturePart> OutputQueue;

        public volatile bool StopThreadsFlag;
        public volatile bool ErrorFlag;
        [JsonIgnore]
        public List<Exception> Errors;

        [JsonIgnore]
        public SpinLock BufferPoolLock;
        [JsonIgnore]
        public SpinLock InputQueueLock;
        [JsonIgnore]
        public SpinLock OutputQueueLock;
        
        [JsonIgnore]
        public Semaphore InputQueueSemaphore;
        [JsonIgnore]
        public AutoResetEvent NextBlockNeededEvent;
        [JsonIgnore]
        public AutoResetEvent NewOutputElementEvent;
        [JsonIgnore]
        public ManualResetEvent StopThreadsEvent;


        public int InputQueueLength
        {
            get
            {
                lock (InputQueue)
                    return InputQueue.Count;
            }
        }

        public int OutputQueueLength
        {
            get
            {
                lock(OutputQueue)
                    return OutputQueue.Count;
            }
        }

        public void Dispose()
        {
            StopThreadsEvent?.Dispose();
            StopThreadsEvent = null;

            NewOutputElementEvent?.Dispose();
            NewOutputElementEvent = null;

            NextBlockNeededEvent?.Dispose();
            NextBlockNeededEvent = null;

            InputQueueSemaphore?.Dispose();
            InputQueueSemaphore = null;

            BufferPool?.Dispose();
            BufferPool = null;
        }
    }
}
