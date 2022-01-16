using System;
using System.Linq;
using System.Threading;
using Akka.Util;
using LinqToDB.Data;

namespace Reactive.Streams.Helpers
{
    public class
        Linq2DbFuncLifetimeProvider<T, TDc> : IContextAsyncEnumerableProvider<T>,
            IContextEnumerableProvider<T>
        where TDc : DataConnection
    {
        private readonly Func<TDc> _dcp;
        private readonly Func<TDc, IQueryable<T>> _qp;

        public Linq2DbFuncLifetimeProvider(
            Func<TDc> dataConnectionProducer,
            Func<TDc, IQueryable<T>> queryableProducer)
        {
            _dcp = dataConnectionProducer;
            _qp = queryableProducer;
        }
        public IAsyncEnumerableContextLifetime<T> GetAsyncLifetime()
        {
            return createLifetime();
        }

        private Linq2DbContextLifetime<T, TDc> createLifetime()
        {
            return new Linq2DbContextLifetime<T, TDc>(_dcp, _qp);
        }

        public IEnumerableContextLifetime<T> GetLifetime()
        {
            return createLifetime();
        }
    }
}