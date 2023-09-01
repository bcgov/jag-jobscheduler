using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Core.Dispatching
{
    /// <summary>
    /// Job dispatcher default implementation
    /// </summary>
    public class JobDispatcher : IJobDispatcher
    {
        private readonly IJobInstanceProvider jobConfigurationProvider;
        private readonly IJobQueueProvider jobQueue;

        /// <summary>
        /// Initialize a new job dispatcher
        /// </summary>
        /// <param name="jobConfigurationProvider"></param>
        /// <param name="jobQueue"></param>
        public JobDispatcher(IJobInstanceProvider jobConfigurationProvider, IJobQueueProvider jobQueue)
        {
            this.jobConfigurationProvider = jobConfigurationProvider;
            this.jobQueue = jobQueue;
        }

        /// <inheritdoc/>
        public virtual async Task Dispatch(CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var pendingJobs = await jobConfigurationProvider.GetReadyJobs(now, ct);
            await jobQueue.Enqueue(pendingJobs, ct);
        }
    }
}