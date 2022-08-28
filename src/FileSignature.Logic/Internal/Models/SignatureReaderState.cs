namespace FileSignature.Logic.Internal.Models
{
    public class SignatureReaderState : IDisposable
    {
        public int BlockSize;
        public long TotalBlocks;

        public FileStream FileStream;
        public Thread ReadingThread;
        public List<Thread> CalculatingThreads;

        public Queue<InputQueueElement> InputQueue;
        public int MaxInputQueueLength;
        public SortedSet<SignaturePart> OutputQueue;

        public volatile bool StopThreadsFlag;
        public volatile bool ErrorFlag;
        public List<Exception> Errors;

        public Semaphore InputQueueSemaphore;
        public AutoResetEvent NextBlockNeededEvent;
        public AutoResetEvent NewOutputElementEvent;
        public ManualResetEvent StopThreadsEvent;
        
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
        }
    }
}
