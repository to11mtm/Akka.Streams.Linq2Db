using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using LinqToDB;
using LinqToDB.Data;

namespace Akka.Linq2Db.Sandbox
{
    public class AsyncPublisherContext<T, TDc> : BasePublisherContext<T>, IAsyncDisposable where TDc:DataConnection
    {
        private readonly Func<TDc, IQueryable<T>> _queryFunc;
        private readonly Func<TDc> _dcFunc;
        private TDc _conn;
        private IAsyncEnumerator<T> _enumerator;
        private CancellationTokenSource _cts;
        private bool isInit;
        public override void CancelToken()
        {
            _cts.Cancel();
        }

        public AsyncPublisherContext(Func<TDc> dcFunc,
            Func<TDc, IQueryable<T>> queryFunc)
        {
            _dcFunc = dcFunc;
            _queryFunc = queryFunc;
            _cts = new CancellationTokenSource();
        }

        public void init()
        {
            _conn = _dcFunc();
            _enumerator = _queryFunc(_conn).AsAsyncEnumerable()
                .GetAsyncEnumerator(_cts.Token);
            isInit = true;
        }

        public override async ValueTask<Option<Try<T>>> ReadNext()
        {
            try
            {
                _cts.Token.ThrowIfCancellationRequested();
                if (isInit == false)
                {
                    init();
                }
                if (await _enumerator.MoveNextAsync())
                {
                    return new Option<Try<T>>(_enumerator.Current);
                }
                else
                {
                    await CloseReader();
                    return Option<Try<T>>.None;
                }
            }
            catch (Exception e)
            {
                return new Option<Try<T>>(new Try<T>(e));
            }
        }

        public override async ValueTask CloseReader()
        {
            await _enumerator.DisposeAsync();
            _conn.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await CloseReader();
        }
    }
}