namespace FileSignature.Logic.Internal.Models
{
    public class SignatureReaderState : IDisposable
    {
        public int BlockSize;
        public int MaxInputQueueLength;

        public FileStream fileStream;
        public Thread readingThread;
        public List<Thread> calculatingThreads;

        public Queue<InputQueueElement> inputQueue;
        public SortedSet<SignaturePart> outputQueue;
        public volatile bool stopThreadsFlag;
        public volatile bool errorFlag;

        public Semaphore inputQueueSemaphore;
        public AutoResetEvent nextBlockNeededEvent;
        public AutoResetEvent newOutputElementEvent;
        public ManualResetEvent stopThreadsEvent;


        public List<Exception> Errors;

        public long totalBlocks;

        public void Dispose()
        {
            fileStream?.Dispose();
            inputQueueSemaphore?.Dispose();
            nextBlockNeededEvent?.Dispose();
            newOutputElementEvent?.Dispose();
            stopThreadsEvent?.Dispose();
        }
    }
}
