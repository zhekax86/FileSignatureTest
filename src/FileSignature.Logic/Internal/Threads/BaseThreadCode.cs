using FileSignature.Logic.Internal.Models;

namespace FileSignature.Logic.Internal.Threads
{
    public abstract class BaseThreadCode
    {
        protected SignatureReaderState State;

        protected BaseThreadCode(SignatureReaderState signatureReaderState)
        {
            State = signatureReaderState;
        }

        public void Run()
        {
            try
            {
                OnRun();
            }
            catch (Exception exception)
            {
                lock(State.Errors)
                    State.Errors.Add(exception);

                State.StopThreadsFlag = true;
                State.ErrorFlag = true;
                State.StopThreadsEvent.Set();
            }
        }

        protected abstract void OnRun();

        protected void LockInputQueue(Action action)
        {
            UsingLock(ref State.InputQueueLock, action);
        }

        protected T LockInputQueue<T>(Func<T> func)
        {
            return UsingLock(ref State.InputQueueLock, func);
        }

        protected void LockBufferPool(Action action)
        {
            UsingLock(ref State.BufferPoolLock, action);
        }
        protected T LockBufferPool<T>(Func<T> func)
        {
            return UsingLock(ref State.BufferPoolLock, func);
        }

        protected void LockOutputQueue(Action action)
        {
            UsingLock(ref State.OutputQueueLock, action);
        }

        protected T LockOutputQueue<T>(Func<T> func)
        {
            return UsingLock(ref State.OutputQueueLock, func);
        }

        private void UsingLock(ref SpinLock spinLock, Action action)
        {
            var lockTaken = false;

            try
            {
                spinLock.Enter(ref lockTaken);
                action();
            }
            finally
            {
                if (lockTaken)
                    spinLock.Exit();
            }
        }

        private T UsingLock<T>(ref SpinLock spinLock, Func<T> func)
        {
            var lockTaken = false;

            try
            {
                spinLock.Enter(ref lockTaken);
                return func();
            }
            finally
            {
                if (lockTaken)
                    spinLock.Exit();
            }
        }
    }
}
