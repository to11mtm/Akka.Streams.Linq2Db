using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;

namespace Reactive.Streams.Helpers
{
    public class EnumerablePublisherContext<T> : BasePublisherContext<T>, IDisposable
    {
        private readonly IContextEnumerableProvider<T> _lifetimeProvider;
        private IEnumerableContextLifetime<T>? _conn;
        private IEnumerator<T> _enumerator;
        private bool isInit = false;
        private readonly CancellationTokenSource _cts;
        private bool isCompleted = false;

        public EnumerablePublisherContext(IContextEnumerableProvider<T> lifetimeProvider)
        {
            _lifetimeProvider = lifetimeProvider;
            _cts = new CancellationTokenSource();
        }

        public void init()
        {
            _conn = _lifetimeProvider.GetLifetime();
            _enumerator = _conn.GetEnumerable().GetEnumerator();
            isInit = true;
            isCompleted = false;
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
                    isCompleted = true;
                    return new ValueTask<Option<Try<T>>>(Option<Try<T>>.None);
                }
            }
            catch (Exception e)
            {
                return new ValueTask<Option<Try<T>>>(new Option<Try<T>>(new Try<T>(e)));
            }
        }

        public override async ValueTask CloseReader()
        {
            _enumerator?.Dispose();
            if (_conn != null) await _conn.DisposeAsync();
        }

        public override void CancelToken()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            if (isCompleted == false)
            {
                CloseReader().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            _cts.Dispose();    
        }
    }
}