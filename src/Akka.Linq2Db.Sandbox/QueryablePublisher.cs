using System;
using System.Linq;
using LinqToDB.Data;
using Reactive.Streams;

namespace Akka.Linq2Db.Sandbox
{
    public class QueryablePublisher<T> : IPublisher<T>
    {
        private readonly Func<BasePublisherContext<T>> _contextFactory;

        

        internal QueryablePublisher(Func<BasePublisherContext<T>> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public void Subscribe(ISubscriber<T> subscriber)
        {
            subscriber.OnSubscribe(new AsyncPublisherContextSubscription<T>(_contextFactory(), subscriber));
        }
    }
    public class QueryablePublisher
    {
        /// <summary>
        /// Creates a <see cref="QueryablePublisher{T}"/> using a given producer function
        /// For the Database connection and <see cref="IQueryable{T}"/> used
        /// </summary>
        /// <param name="connFact"></param>
        /// <param name="producer"></param>
        /// <typeparam name="TDc"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static QueryablePublisher<T> From<TDc, T>(Func<TDc> connFact,
            Func<TDc, IQueryable<T>> producer) where TDc : DataConnection
        {
            return new QueryablePublisher<T>(() =>
                new PublisherContext<T, TDc>(connFact, producer));
        }
        public static QueryablePublisher<T> FromAsync<TDc, T>(Func<TDc> connFact,
            Func<TDc, IQueryable<T>> producer) where TDc : DataConnection
        {
            return new QueryablePublisher<T>(() =>
                new AsyncPublisherContext<T, TDc>(connFact, producer));
        }
    }
}