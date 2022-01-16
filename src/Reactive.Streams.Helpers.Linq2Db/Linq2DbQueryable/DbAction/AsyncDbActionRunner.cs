using System;
using System.Threading.Tasks;
using LinqToDB.Data;

namespace Reactive.Streams.Helpers
{
    public class AsyncDbActionRunner<T, TDc> where TDc: DataConnection
    {
        private readonly Func<TDc,T, Task> _actionRunner;
        private readonly Func<TDc> _connectionProvider;
        private TDc _connectionInstance;
        public AsyncDbActionRunner(Func<TDc> connectionProvider,
            Func<TDc,T, Task> actionRunner)
        {
            _connectionProvider = connectionProvider;
            _actionRunner = actionRunner;
        }

        public async ValueTask RunAction(T item)
        {
            using (var dc = _connectionProvider())
            {
                await _actionRunner(dc, item);
            }
        }
    }
}