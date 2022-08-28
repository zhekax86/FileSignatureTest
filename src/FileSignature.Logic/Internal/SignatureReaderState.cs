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

        public FileStream fileStream;
        public Thread readingThread;
        public List<Thread> calculatingThreads;

        public Queue<InputQueueElement> inputQueue;
        public SortedSet<SignaturePart> outputQueue;

        public Semaphore inputQueueSemaphore;
        public AutoResetEvent nextBlockNeededEvent;
        
        public long totalBlocks;

        public void Dispose()
        {
            fileStream?.Dispose();
            inputQueueSemaphore?.Dispose();
            nextBlockNeededEvent?.Dispose();
        }
    }
}
