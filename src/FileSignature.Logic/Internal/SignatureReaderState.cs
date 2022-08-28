using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic.Internal
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

        public Semaphore inputQueueSemaphore;
        public AutoResetEvent nextBlockNeededEvent;
        public AutoResetEvent newOutputElementEvent;
        
        public long totalBlocks;

        public void Dispose()
        {
            fileStream?.Dispose();
            inputQueueSemaphore?.Dispose();
            nextBlockNeededEvent?.Dispose();
            newOutputElementEvent?.Dispose();
        }
    }
}
