using System.Security.Cryptography;
using FileSignature.Logic.Internal.Models;

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

                if (State.StopThreadsFlag)
                    return;

                var element = LockInputQueue(() => State.InputQueue.Dequeue());
                State.NextBlockNeededEvent.Set();

                var signaturePart = new SignaturePart
                {
                    PartNumber = element.BlockNumber,
                    TotalParts = State.TotalBlocks,
                    Hash = hashAlgorithm.ComputeHash(element.Buffer, 0, element.BufferLength)
                };

                LockBufferPool(() => State.BufferPool.Return(element.Buffer));
                LockOutputQueue(() => State.OutputQueue.Add(signaturePart));
                State.NewOutputElementEvent.Set();
            }
        }

        private void WaitForNewElement()
        {
            _waitHandlers ??= new WaitHandle[]
            {
                State.StopThreadsEvent,
                State.InputQueueSemaphore
            };

            WaitHandle.WaitAny(_waitHandlers);
        }
    }
}
