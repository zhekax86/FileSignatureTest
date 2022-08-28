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
                State.StopThreadsEvent.Set();
            }
        }

        protected abstract void OnRun();
    }
}
