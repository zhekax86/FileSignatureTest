namespace FileSignature.Logic.Internal
{
    public class BufferPool : IDisposable
    {
        private int _bufferSize;
        private readonly Queue<byte[]> _freeBuffers;
        
        public BufferPool(int bufferSize)
        {
            _bufferSize = bufferSize;
            _freeBuffers = new();
        }

        public byte[] Get()
        {
            if (_freeBuffers.Count == 0)
                return new byte[_bufferSize];

            return _freeBuffers.Dequeue();
        }

        public void Return(byte[] buffer)
        { 
            _freeBuffers.Enqueue(buffer);
        }

        public void Dispose()
        {
            _freeBuffers.Clear();
        }
    }
}
