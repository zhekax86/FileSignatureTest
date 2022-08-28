﻿namespace FileSignature.Logic.Internal.Threads
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

                State.stopThreadsFlag = true;
                State.stopThreadsEvent.Set();
            }
        }

        protected abstract void OnRun();
    }
}