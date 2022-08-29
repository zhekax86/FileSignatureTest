using FileSignature.Logic.Internal.Models;

namespace FileSignature.Logic.Internal.Threads
{
    public class ReadingThreadCode : BaseThreadCode
    {
        private WaitHandle[] _waitHandlers;

        public ReadingThreadCode(SignatureReaderState state) 
            :base(state)
        {
            State = state;
        }
        protected override void OnRun()
        {
            long currentBlockNumber = 1L;

            while (State.FileStream.Position < State.FileStream.Length)
            {
                WaitIfInputQueueIsFull();

                if (State.StopThreadsFlag)
                    return;
                
                var element = new InputQueueElement
                {
                    BlockNumber = currentBlockNumber++,
                    Buffer = LockBufferPool(() => State.BufferPool.Get())
                };

                element.BufferLength = State.FileStream
                    .Read(element.Buffer, 0, State.BlockSize);

                LockInputQueue(() => State.InputQueue.Enqueue(element));

                State.InputQueueSemaphore.Release();
            }
        }

        private void WaitIfInputQueueIsFull()
        {
            int queueLength = 0;

            queueLength = LockInputQueue(() => State.InputQueue.Count);

            if (queueLength >= State.MaxInputQueueLength)
                WaitEvents();
        }

        private void WaitEvents()
        {
            _waitHandlers ??= new WaitHandle[]
            {
                State.StopThreadsEvent,
                State.NextBlockNeededEvent
            };

            WaitHandle.WaitAny(_waitHandlers);
        }
    }
}
