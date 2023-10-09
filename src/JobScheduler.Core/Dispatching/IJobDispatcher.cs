namespace JobScheduler.Core.Dispatching
{
    /// <summary>
    /// Job dispatcher that enqueue jobs that are ready for execution in the job queue
    /// </summary>
    public interface IJobDispatcher
    {
        /// <summary>
        /// Dispatch ready to be queued jobs
        /// </summary>
        /// <returns></returns>
        Task Dispatch(CancellationToken ct = default);
    }
}