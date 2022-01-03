using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;
using LinqToDB.Data;

namespace Akka.Linq2Db.Sandbox
{
    public class PublisherContext<T, TDc> : BasePublisherContext<T>, IDisposable where TDc:DataConnection
    {
        private readonly Func<TDc, IQueryable<T>> _queryFunc;
        private readonly Func<TDc> _dcFunc;
        private TDc _conn;
        private IEnumerator<T> _enumerator;
        private bool isInit = false;
        private readonly CancellationTokenSource _cts;

        public PublisherContext(Func<TDc> dcFunc,
            Func<TDc, IQueryable<T>> queryFunc)
        {
            _dcFunc = dcFunc;
            _queryFunc = queryFunc;
            _cts = new CancellationTokenSource();
        }

        public void init()
        {
            _conn = _dcFunc();
            _enumerator = _queryFunc(_conn).GetEnumerator();
            isInit = true;
        }

        public override ValueTask<Option<Try<T>>> ReadNext()
        {
            try
            {
                _cts.Token.ThrowIfCancellationRequested();
                if (isInit == false)
                {
                    init();
                }
                if (_enumerator.MoveNext())
                {
                    return new ValueTask<Option<Try<T>>>(new Option<Try<T>>(_enumerator.Current));
                }
                else
                {
                    CloseReader();
                    return new ValueTask<Option<Try<T>>>(Option<Try<T>>.None);
                }
            }
            catch (Exception e)
            {
                return new ValueTask<Option<Try<T>>>(new Option<Try<T>>(new Try<T>(e)));
            }
        }

        public override ValueTask CloseReader()
        {
            _enumerator.Dispose();
            _conn.Dispose();
            return new ValueTask();
        }

        public override void CancelToken()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            CloseReader();
            _cts.Dispose();
        }
    }
}