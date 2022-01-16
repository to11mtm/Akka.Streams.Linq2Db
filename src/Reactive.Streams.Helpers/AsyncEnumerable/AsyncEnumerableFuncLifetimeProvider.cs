using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reactive.Streams.Helpers
{
    public class
        AsyncEnumerableFuncLifetimeProvider<T> : IAsyncEnumerableContextLifetime
            <T>
    {
        private readonly IAsyncEnumerable<T> _provider;

        public AsyncEnumerableFuncLifetimeProvider(IAsyncEnumerable<T> provider)
        {
            _provider = provider;
        }

        public async ValueTask DisposeAsync()
        {
            
        }

        public IAsyncEnumerable<T> GetAsyncEnumerable()
        {
            return _provider;
        }
    }
}