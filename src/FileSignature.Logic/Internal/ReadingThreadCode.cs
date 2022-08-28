using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic.Internal
{
    public class ReadingThreadCode
    {
        private readonly SignatureReaderState _state;

        public ReadingThreadCode(SignatureReaderState state)
        {
            _state = state;
        }

        public void Run()
        {
            long currentBlockNumber = 0L;

            while (_state.fileStream.Position < _state.fileStream.Length)
            {
                WaitIfInputQueueIsFull();

                var element = new InputQueueElement
                {
                    BlockNumber = currentBlockNumber++,
                    buffer = new byte[_state.BlockSize]
                };

                element.bufferLength = _state.fileStream
                    .Read(element.buffer, 0, _state.BlockSize);

                lock (_state.inputQueue)
                    _state.inputQueue.Enqueue(element);
                
                _state.inputQueueSemaphore.Release();
            }
        }

        public void WaitIfInputQueueIsFull()
        {
            int queueLength;

            lock (_state.inputQueue)
                queueLength = _state.inputQueue.Count;

            if (queueLength >= _state.MaxInputQueueLength)
                _state.nextBlockNeededEvent.WaitOne();
        }
    }
}
