using Microsoft.Extensions.Logging;

namespace JobScheduler.Core.Dispatching
{
    /// <summary>
    /// Job dispatcher default implementation
    /// </summary>
    public class JobDispatcher : IJobDispatcher
    {
        private readonly IJobInstanceProvider jobConfigurationProvider;
        private readonly IJobQueueProvider jobQueue;
        private readonly ILogger<JobDispatcher> logger;

        /// <summary>
        /// Initialize a new job dispatcher
        /// </summary>
        /// <param name="jobConfigurationProvider"></param>
        /// <param name="jobQueue"></param>
        /// <param name="logger"></param>
        public JobDispatcher(IJobInstanceProvider jobConfigurationProvider, IJobQueueProvider jobQueue, ILogger<JobDispatcher> logger)
        {
            this.jobConfigurationProvider = jobConfigurationProvider;
            this.jobQueue = jobQueue;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task Dispatch(CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var pendingJobs = await jobConfigurationProvider.GetReadyJobs(now, ct);
            await jobQueue.Enqueue(pendingJobs, ct);
            logger.LogInformation("Enqueued {Jobs} jobs", pendingJobs.Count());
        }
    }
}