using System;
using Reactive.Streams;

namespace Reactive.Streams.Helpers
{
    public class ContextBasedPublisher<T> : IPublisher<T>
    {
        private readonly Func<BasePublisherContext<T>> _contextFactory;
        private readonly ISchedulerFactory? _schedulerFactory;


        public ContextBasedPublisher(Func<BasePublisherContext<T>> contextFactory, ISchedulerFactory? schedulerFactory)
        {
            _contextFactory = contextFactory;
            _schedulerFactory = schedulerFactory;
        }
        public void Subscribe(ISubscriber<T> subscriber)
        {
            subscriber.OnSubscribe(new AsyncPublisherContextSubscription<T>(
                _contextFactory(), subscriber, _schedulerFactory?.Get()));
        }
    }
}