using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic.Internal
{
    public class SignatureReader
    {
        private readonly string _fileName;
        private readonly int _blockSize;
        private SignatureReaderState _state;
                
        public SignatureReader(string fileName, int blockSize)
        {
            _fileName = fileName;
            _blockSize = blockSize;
        }


        public IEnumerable<SignaturePart> Read()
        {
            using(_state = new SignatureReaderState())
            {
                _state.fileStream = File.OpenRead(_fileName);

                _state.totalBlocks = CalcTotalBlocks(_state.fileStream.Length);
                var calculatingThreadsCount = GetCalculatingThreadsCount(_state.totalBlocks);

                _state.inputQueue = new Queue<InputQueueElement>(calculatingThreadsCount);
                _state.outputQueue = new SortedSet<SignaturePart>();

                _state.readingThread = new Thread(ReadingThreadCode);
                _state.readingThread.Start();

                _state.calculatingThreads = new List<Thread>(calculatingThreadsCount);
                for (int i = 0; i < calculatingThreadsCount; i++)
                {
                    var calculatingThread = new Thread(CalculatingThreadCode);
                    calculatingThread.Start();

                    _state.calculatingThreads.Add(calculatingThread);
                }

                // wait for output queue

                
            }

            yield return new SignaturePart();
        }

        public long CalcTotalBlocks(long fileSize)
        {
            var result = fileSize / _blockSize;
            if (fileSize % _blockSize != 0)
                result++;

            return result;
        }

        public static int GetCalculatingThreadsCount(long blocksCount)
        {
            var processorCount = Environment.ProcessorCount;

            if (blocksCount > (long)processorCount)
                return processorCount;

            return (int)blocksCount;

        }

        public void ReadingThreadCode()
        {
            long currentBlockNumber = 0L;

            while(_state.fileStream.Position < _state.fileStream.Length)
            {
                // wait here if queue is full

                var element = new InputQueueElement
                {
                    BlockNumber = currentBlockNumber++,
                    buffer = new byte[_blockSize]
                };

                element.bufferLength = _state.fileStream
                    .Read(element.buffer, 0, _blockSize);

                _state.inputQueue.Enqueue(element);
                // notice calculation threads
            }
        }

        public void CalculatingThreadCode()
        {
            using var hashAlgorithm = SHA256.Create();
            var enough = false;

            while (!enough)
            {
                // wait for new element

                var element = _state.inputQueue.Dequeue();

                var signaturePart = new SignaturePart
                {
                    PartNumber = element.BlockNumber,
                    TotalParts = _state.totalBlocks,
                    Hash = hashAlgorithm.ComputeHash(element.buffer, 0, element.bufferLength)
                };

                _state.outputQueue.Add(signaturePart);

                // notice reader to read next block

            }
        }
    }
}
