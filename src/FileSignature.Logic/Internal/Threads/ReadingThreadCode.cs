using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            while (State.fileStream.Position < State.fileStream.Length)
            {
                WaitIfInputQueueIsFull();

                if (State.stopThreadsFlag)
                    return;
                
                var element = new InputQueueElement
                {
                    BlockNumber = currentBlockNumber++,
                    buffer = new byte[State.BlockSize]
                };

                element.bufferLength = State.fileStream
                    .Read(element.buffer, 0, State.BlockSize);

                lock (State.inputQueue)
                    State.inputQueue.Enqueue(element);

                State.inputQueueSemaphore.Release();
            }
        }

        private void WaitIfInputQueueIsFull()
        {
            int queueLength;

            lock (State.inputQueue)
                queueLength = State.inputQueue.Count;

            if (queueLength >= State.MaxInputQueueLength)
                WaitEvents();
        }

        private void WaitEvents()
        {
            _waitHandlers ??= new WaitHandle[]
            {
                State.stopThreadsEvent,
                State.nextBlockNeededEvent
            };

            WaitHandle.WaitAny(_waitHandlers);
        }
        
    }
}
