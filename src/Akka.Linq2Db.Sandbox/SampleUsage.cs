using Akka.Streams.Dsl;
using LinqToDB.Data;

namespace Akka.Linq2Db.Sandbox
{
    public class SampleUsage
    {
        public void Sample()
        {
            Source.FromPublisher(QueryablePublisher.From(
                () => new DataConnection(),
                dc =>
                {
                    return dc.GetTable<StrawMan>();
                }));
        }
    }
}