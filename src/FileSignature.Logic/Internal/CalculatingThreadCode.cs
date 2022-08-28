﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic.Internal
{
    public class CalculatingThreadCode
    {
        private readonly SignatureReaderState _state;

        public CalculatingThreadCode(SignatureReaderState state)
        {
            _state = state;
        }

        public void Run()
        {
            using var hashAlgorithm = SHA256.Create();
            var enough = false;

            while (!enough)
            {
                // wait for new element
                _state.inputQueueSemaphore.WaitOne();

                var element = _state.inputQueue.Dequeue();

                var signaturePart = new SignaturePart
                {
                    PartNumber = element.BlockNumber,
                    TotalParts = _state.totalBlocks,
                    Hash = hashAlgorithm.ComputeHash(element.buffer, 0, element.bufferLength)
                };

                _state.outputQueue.Add(signaturePart);

                // notice reader to read next block
                _state.nextBlockNeededEvent.Set();
            }
        }
    }
}