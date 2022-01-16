using System.Threading.Tasks;

namespace Reactive.Streams.Helpers
{
    /// <summary>
    /// Provides a TaskScheduler for Stream Request Handlers to process
    /// </summary>
    public interface ISchedulerFactory
    {
        /// <summary>
        /// Gets the TaskScheduler that will be used
        /// For scheduling publisher actions
        /// </summary>
        /// <returns></returns>
        TaskScheduler Get();
    }
}