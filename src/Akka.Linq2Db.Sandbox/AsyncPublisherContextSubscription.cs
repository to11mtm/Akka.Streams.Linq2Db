using System;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Streams;

namespace Akka.Linq2Db.Sandbox
{
    public class AsyncPublisherContextSubscription<T> : ISubscription
    {
        private readonly BasePublisherContext<T> _obj;
        private readonly ISubscriber<T> _sub;
        private readonly TaskScheduler _scheduler;
        public AsyncPublisherContextSubscription(BasePublisherContext<T> obj,
            ISubscriber<T> subscriber, TaskScheduler? scheduler)
        {
            _obj = obj;
            _sub = subscriber;
            _scheduler = scheduler ?? TaskScheduler.Default;
        }

        public AsyncPublisherContextSubscription(BasePublisherContext<T> obj,
            ISubscriber<T> subscriber) : this(obj, subscriber, null)
        {
            
        }

        private long _pendingReqs;
        
        private int taskState = 0;
        private bool cancelled = false;
        private const int TaskStopped = 0;
        private const int TaskRunning = 1 << 1;
        private void TryCloseTaskRead()
        {
            Volatile.Write(ref taskState, TaskStopped);
            if (Interlocked.Read(ref _pendingReqs) > 0)
            {
                TryRequestReader();
            }
        }
        public void Request(long n)
        {
            Interlocked.Add(ref _pendingReqs, n);
            if (Volatile.Read(ref taskState) ==TaskStopped)
            {
                TryRequestReader();
            }
            
        }

        private void TryRequestReader()
        {
            if (Interlocked.CompareExchange(ref taskState, TaskRunning,
                    TaskStopped) == TaskStopped)
            {
                StartRequestReader();
            }
        }

        private void StartRequestReader()
        {
            Task.Factory.StartNew(async () =>
            {
                var currReqs = Interlocked.Exchange(ref _pendingReqs, 0);
                do
                {
                    try
                    {
                        for (long i = 0; i < currReqs; i++)
                        {
                            var next = await _obj.ReadNext();
                            if (next.IsEmpty)
                            {
                                _sub.OnComplete();
                            }
                            else
                            {
                                _sub.OnNext(next.Value.Get());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _sub.OnError(e);
                    }

                    currReqs = Interlocked.Exchange(ref _pendingReqs, 0);
                } while (currReqs > 0 && Volatile.Read(ref cancelled)==false);

                TryCloseTaskRead();
            });
        }

        

        public void Cancel()
        {
            _obj.CancelToken();
            Volatile.Write(ref cancelled, true);
        }
    }
}