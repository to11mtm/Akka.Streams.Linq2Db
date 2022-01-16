namespace Reactive.Streams.Helpers
{
    /// <summary>
    /// Provides a Context (scoped) AsyncEnumerable Lifetime
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IContextAsyncEnumerableProvider<out T>
    {
        IAsyncEnumerableContextLifetime<T> GetAsyncLifetime();
    }

    /// <summary>
    /// Provides a Context (scoped) enumerable lifetime 
    /// </summary>
    /// <typeparam name="T">The type of Elements in the Enumerable</typeparam>
    public interface IContextEnumerableProvider<out T>
    {
        IEnumerableContextLifetime<T> GetLifetime();
    }
}