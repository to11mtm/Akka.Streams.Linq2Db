using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;

namespace Reactive.Streams.Helpers
{
    public class Linq2DbContextLifetime<T,TDc> : 
        IAsyncEnumerableContextLifetime<T>,
        IEnumerableContextLifetime<T> 
        where TDc:DataConnection
    {
        private readonly Func<TDc, IQueryable<T>> _qp;
        private readonly TDc _dcp;

        public Linq2DbContextLifetime(
            Func<TDc> dataConnectionProducer,
            Func<TDc, IQueryable<T>> queryableProducer)
        {
            _dcp = dataConnectionProducer();
            _qp = queryableProducer;
        }
        public ValueTask DisposeAsync()
        {
            _dcp?.Dispose();
            return new ValueTask();
        }

        public IEnumerable<T> GetEnumerable()
        {
            return _qp(_dcp);
        }

        public IAsyncEnumerable<T> GetAsyncEnumerable()
        {
            return _qp(_dcp).AsAsyncEnumerable();
        }
    }
}