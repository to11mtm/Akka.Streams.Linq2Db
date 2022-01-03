using System;
using System.Threading.Tasks;
using Akka.Util;
using Google.Protobuf.WellKnownTypes;
using Reactive.Streams;

namespace Akka.Linq2Db.Sandbox
{

    public abstract class BasePublisherContext<T>
    {

        public abstract ValueTask<Option<Try<T>>> ReadNext();

        public abstract ValueTask CloseReader();
        public abstract void CancelToken();
    }
}