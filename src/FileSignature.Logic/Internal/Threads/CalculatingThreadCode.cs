using System.Security.Cryptography;

namespace FileSignature.Logic.Internal.Threads
{
    public class CalculatingThreadCode : BaseThreadCode
    {
        private WaitHandle[] _waitHandlers;

        public CalculatingThreadCode(SignatureReaderState state)
            : base(state)
        {
        }

        protected override void OnRun()
        {
            using var hashAlgorithm = SHA256.Create();

            while (true)
            {
                WaitForNewElement();

                if (State.stopThreadsFlag)
                    return;

                InputQueueElement element;
                lock (State.inputQueue)
                    element = State.inputQueue.Dequeue();
                State.nextBlockNeededEvent.Set();

                var signaturePart = new SignaturePart
                {
                    PartNumber = element.BlockNumber,
                    TotalParts = State.totalBlocks,
                    Hash = hashAlgorithm.ComputeHash(element.buffer, 0, element.bufferLength)
                };

                lock (State.outputQueue)
                    State.outputQueue.Add(signaturePart);

                // notice reader to read next block
                State.newOutputElementEvent.Set();
            }
        }

        private void WaitForNewElement()
        {
            _waitHandlers ??= new WaitHandle[]
            {
                State.stopThreadsEvent,
                State.inputQueueSemaphore
            };

            WaitHandle.WaitAny(_waitHandlers);
        }
    }
}
