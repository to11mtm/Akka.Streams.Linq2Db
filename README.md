# Akka.Streams.Linq2Db (And some Great Reactive Streams bits)

This repo is part of a project to provide a form of Reactive Streams adaptation for Linq2Db. Ideally, With a DSL that works well for Akka.Streams.

## Reactive Sterams Publishers

The `Reactive.Streams.Helpers` Project contains helpers to Create `IPublisher<T>` instances that work off `IEnumerable<T>` or `IAsyncEnumerable<T>`

#### How?:

Lets say we want to use an `IAsyncEnumerable` as an Akka Streams Source.
```c#
using Reactive.Streams.Helpers.DSL;
using Akka.Streams.Dsl;

//In some class
var myAsyncEnumerableSource = Source.FromPublisher(
                                  AsyncEnumerablePublisher.From(
                                    producer: ()=> GetsAnAsyncEnumerable()
                                    schedulerFactory: null
                                    )
                                  );
//Use it in a graph
```

If you want something more complex, for instance, You need a lifetime, and to dispose, you can Implement `IContextAsyncEnumerableProvider<T>` and `IAsyncEnumerableContextLifetime<T>`.

 - `IContextAsyncEnumerableProvider<T>` produces a `IAsyncEnumerableContextLifetime<T>` for each subscription (e.x. materialization of an Akka Stream.)
 - `IAsyncEnumerableContextLifetime<T>` Will have `GetAsyncEnumerable()` called -once-, and will have `DisposeAsync()` called when the enumerable is out of data, or the stream is cancelled.

`ISchedulerFactory` is intended to Provide a `TaskScheduler` for each lifetime (materialization.) If none is provided, `TaskScheduler.Default` will wind up being used.

## How it works:

So, we have this idea of a `ContextBasedPublisher<T>`. It Takes a `Func<BasePublisherContext<T>>` and `ISchedulerFactory`.

When `.Subscribe()` is called, each of those factorys are called and given to `AsyncPublisherContextSubscription<T>`.

`AsyncPublisherContextSubscription<T>` basically works to manage handling Requests from the subscriber Asynchronusly; However, since `BasePublisherContext<T>` may not be fully Async in implementation, the option to give a custom `TaskScheduler` is provided.

`AsyncPublisherContextSubscription<T>` operates in such a way that Reads from the enumerable are handled on the scheduled task; This is done to solve the 'reentrancy' concerns that come from Subscriber `OnNext` making additional calls to `Request`.  `Request` will increment the pending counter, and check to see whether the reader task needs to be started again, but that's about it.