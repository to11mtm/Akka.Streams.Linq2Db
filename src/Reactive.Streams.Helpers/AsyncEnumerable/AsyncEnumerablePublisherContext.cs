using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Akka.Util;

namespace Reactive.Streams.Helpers
{
    public class AsyncEnumerablePublisherContext<T> : BasePublisherContext<T>, IAsyncDisposable
    {
        private readonly IContextAsyncEnumerableProvider<T> _lifetimeProvider; 
        private IAsyncEnumerableContextLifetime<T>? _conn;
        private IAsyncEnumerator<T>? _enumerator;
        private CancellationTokenSource _cts;
        private bool isInit;
        private bool isCompleted;

        public override void CancelToken()
        {
            _cts.Cancel();
        }

        public AsyncEnumerablePublisherContext(IContextAsyncEnumerableProvider<T> lifetimeProvider)
        {
            _lifetimeProvider = lifetimeProvider;
            _cts = new CancellationTokenSource();
        }

        public void init()
        {
            _conn = _lifetimeProvider.GetAsyncLifetime();
            _enumerator = _conn.GetAsyncEnumerable().GetAsyncEnumerator();
            isInit = true;
            isCompleted = false;
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

                Debug.Assert(_enumerator != null, nameof(_enumerator) + " != null");
                if (await _enumerator.MoveNextAsync())
                {
                    return new Option<Try<T>>(_enumerator.Current);
                }
                else
                {
                    await CloseReader();
                    isCompleted = true;
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
            if (_enumerator != null) await _enumerator.DisposeAsync();
            if (_conn != null) await _conn.DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (isCompleted == false)
            {
                await CloseReader();    
            }
            _cts.Dispose();
        }
    }
}