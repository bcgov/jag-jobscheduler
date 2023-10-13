using JobScheduler.Core.Reporting;
using Microsoft.Extensions.Logging;

namespace JobScheduler.Core.Execution
{
    /// <summary>
    /// default implementation of job execution agent
    /// </summary>
    public class JobExecutionAgent : IJobExecutionAgent
    {
        private readonly IJobQueueProvider jobQueue;
        private readonly IJobStateReporter jobStateReporter;
        private readonly IEnumerable<IJobExecuterFactory> jobExecuterFactories;
        private readonly ILogger<JobExecutionAgent> logger;

        /// <summary>
        /// Initializes a new job execution agent
        /// </summary>
        /// <param name="jobQueue"></param>
        /// <param name="jobStateReporter"></param>
        /// <param name="jobExecuterFactories"></param>
        /// <param name="logger"></param>
        public JobExecutionAgent(IJobQueueProvider jobQueue, IJobStateReporter jobStateReporter, IEnumerable<IJobExecuterFactory> jobExecuterFactories, ILogger<JobExecutionAgent> logger)
        {
            this.jobQueue = jobQueue;
            this.jobStateReporter = jobStateReporter;
            this.jobExecuterFactories = jobExecuterFactories;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task Process(CancellationToken ct = default)
        {
            var job = await jobQueue.Dequeue(null, ct);
            if (job == null)
            {
                return;
            }

            logger.LogDebug("Executing job {JobId}", job.JobId);
            var executerFactory = jobExecuterFactories.SingleOrDefault(f => f.CanCreateFor(job));
            if (executerFactory == null)
            {
                throw new InvalidOperationException($"Could not find job executor for execution strategy {job.JobDescription.ExecutionStrategy.GetType().Name}");
            }

            var executer = await executerFactory.Create(job, ct);

            var result = await executer.Execute(job, ct);
            await jobStateReporter.Report(result, ct);
            logger.LogInformation("Job {JobId} executed", job.JobId);
            if (!result.Success)
            {
                logger.LogError("Job {JobId} failed: {Error}", job.JobId, result.Error?.Message);
                throw new JobExecutionException($"Execution failed: {result.Error?.Message}") { JobId = job.JobId };
            }
        }
    }
}