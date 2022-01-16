using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Streams.Dsl;
using Reactive.Streams;

namespace Reactive.Streams.Helpers
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

        private long _pendingReqs;
        
        private int taskState = 0;
        private bool cancelled = false;
        /// <summary>
        /// need to track Completion State
        /// Since subsequent Requests should not
        /// be ignored after complete.
        /// </summary>
        private bool completed = false;
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
            if (completed == false)
            {

                Task.Factory.StartNew(async () =>
                    {
                        var currReqs =
                            Interlocked.Exchange(ref _pendingReqs, 0);
                        do
                        {
                            try
                            {
                                for (long i = 0; i < currReqs; i++)
                                {
                                    var nextRead = await _obj.ReadNext();
                                    if (nextRead.IsEmpty)
                                    {
                                        
                                        completed = true;
                                        _sub.OnComplete();
                                    }
                                    else
                                    {
                                        _sub.OnNext(nextRead.Value.Get());
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                
                                completed = true;
                                _sub.OnError(e);
                            }

                            currReqs =
                                Interlocked.Exchange(ref _pendingReqs, 0);
                        } while (completed == false && currReqs > 0 &&
                                 Volatile.Read(ref cancelled) == false);

                        TryCloseTaskRead();
                    }, CancellationToken.None,
                    TaskCreationOptions.RunContinuationsAsynchronously,
                    _scheduler);

            }
        }



        public void Cancel()
        {
            _obj.CancelToken();
            Volatile.Write(ref cancelled, true);
        }
    }
}