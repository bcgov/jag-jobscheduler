using JobScheduler.Core.Execution;

namespace JobScheduler.Core
{
    /// <summary>
    /// A queue provider for scheduled jobs
    /// </summary>
    public interface IJobQueueProvider
    {
        /// <summary>
        /// Enqueue job innstances
        /// </summary>
        /// <param name="jobs"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task Enqueue(IEnumerable<JobInstance> jobs, CancellationToken ct = default);

        /// <summary>
        /// Dequeue job instances from the queue
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<JobInstance?> Dequeue(JobInstanceFilter? filter = null, CancellationToken ct = default);
    }

    /// <summary>
    /// Represents a filter for job instances in queue
    /// </summary>
    public abstract record JobInstanceFilter();
}
