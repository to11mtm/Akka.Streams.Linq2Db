using System;
using System.Collections.Generic;
using Akka.Streams.Dsl;

namespace Reactive.Streams.Helpers.DSL
{
    public class AsyncEnumerablePublisher
    {
        public static ContextBasedPublisher<T> From<T>(
            Func<IAsyncEnumerable<T>> producer,ISchedulerFactory? schedulerFactory=null)
        {
            return new ContextBasedPublisher<T>(() =>
                new AsyncEnumerablePublisherContext<T>(
                    new AsyncEnumerableFuncProvider<T>(producer)), schedulerFactory);
        }
        
        public static ContextBasedPublisher<T> From<T>(
            IContextAsyncEnumerableProvider<T> producer, ISchedulerFactory? schedulerFactory=null)
        {
            return new ContextBasedPublisher<T>(() =>
                new AsyncEnumerablePublisherContext<T>(
                    producer), schedulerFactory);
        }
    }
}