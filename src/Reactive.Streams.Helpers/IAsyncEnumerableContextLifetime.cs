using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reactive.Streams.Helpers
{
    /// <summary>
    /// Provides an Enumerable alongside the provided context
    /// The intent is For `GetEnumerable` to only be called once per Lifetime
    /// And, of Course, for `DisposeAsync` to be called once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEnumerableContextLifetime<out T>
    {
        ValueTask DisposeAsync();
        IEnumerable<T> GetEnumerable();
    }
    /// <summary>
    /// Provides an AsyncEnumerable alongside the provided context
    /// The intent is For `GetAsyncEnumerable` to only be called once per Lifetime
    /// And, of Course, for `DisposeAsync` to be called once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncEnumerableContextLifetime<out T>
    {
        ValueTask DisposeAsync();
        IAsyncEnumerable<T> GetAsyncEnumerable();
    }
}