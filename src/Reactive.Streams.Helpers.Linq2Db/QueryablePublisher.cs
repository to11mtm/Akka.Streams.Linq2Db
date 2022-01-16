using System;
using System.Linq;
using LinqToDB.Data;

namespace Reactive.Streams.Helpers.DSL
{
    public class QueryablePublisher
    {
        /// <summary>
        /// Creates a <see cref="QueryablePublisher{T}"/> using a given producer function
        /// For the Database connection and <see cref="IQueryable{T}"/> used
        /// </summary>
        public static ContextBasedPublisher<T> From<TDc, T>(Func<TDc> connFact,
            Func<TDc, IQueryable<T>> producer, ISchedulerFactory? schedulerFactory=null) where TDc : DataConnection
        {
            return new ContextBasedPublisher<T>(() =>
                new EnumerablePublisherContext<T>(
                    new Linq2DbFuncLifetimeProvider<T, TDc>(connFact,
                        producer)), schedulerFactory);
        }
        public static ContextBasedPublisher<T> FromAsync<TDc, T>(Func<TDc> connFact,
            Func<TDc, IQueryable<T>> producer, ISchedulerFactory? schedulerFactory=null) where TDc : DataConnection
        {
            return new ContextBasedPublisher<T>(() =>
                new AsyncEnumerablePublisherContext<T>(
                    new Linq2DbFuncLifetimeProvider<T, TDc>(
                        connFact, producer)
                ), schedulerFactory);
        }
    }
}