using System;
using System.Collections.Generic;

namespace Reactive.Streams.Helpers
{
 
    /// <summary>
    /// Allows a Producer function for <see cref="IAsyncEnumerable{T}"/> to be used
    /// For Reactive Streams Publishing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class
        AsyncEnumerableFuncProvider<T> : IContextAsyncEnumerableProvider<T>
    {
        private readonly Func<IAsyncEnumerable<T>> _provider;

        public AsyncEnumerableFuncProvider(Func<IAsyncEnumerable<T>> provider)
        {
            _provider = provider;
        }
        public IAsyncEnumerableContextLifetime<T> GetAsyncLifetime()
        {
            return new AsyncEnumerableFuncLifetimeProvider<T>(_provider());
        }
    }
}