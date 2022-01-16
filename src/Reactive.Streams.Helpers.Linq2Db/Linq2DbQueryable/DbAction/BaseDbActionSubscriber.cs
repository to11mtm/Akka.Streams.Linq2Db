using System;
using Reactive.Streams;

namespace Reactive.Streams.Helpers
{
    public abstract class BaseDbActionSubscriber<T> : ISubscriber<T>
    {
        public void OnSubscribe(ISubscription subscription)
        {
            
        }
        
        public void OnNext(T element)
        {
            HandleRecord(element);
        }

        protected abstract void HandleRecord(T element);

        protected abstract void StartTask();

        public void OnError(Exception cause)
        {
            throw new NotImplementedException();
        }

        public void OnComplete()
        {
            throw new NotImplementedException();
        }
    }
}