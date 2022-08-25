using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic.Internal
{
    public class SignatureReaderState : IDisposable
    {
        public FileStream fileStream;
        public Thread readingThread;
        public List<Thread> calculatingThreads;

        public long totalBlocks;

        public void Dispose()
        {
            fileStream?.Dispose();
        }
    }
}
