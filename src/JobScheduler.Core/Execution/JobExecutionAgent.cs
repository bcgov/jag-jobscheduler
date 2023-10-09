using JobScheduler.Core.Reporting;

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

        /// <summary>
        /// Initializes a new job execution agent
        /// </summary>
        /// <param name="jobQueue"></param>
        /// <param name="jobStateReporter"></param>
        /// <param name="jobExecuterFactories"></param>
        public JobExecutionAgent(IJobQueueProvider jobQueue, IJobStateReporter jobStateReporter, IEnumerable<IJobExecuterFactory> jobExecuterFactories)
        {
            this.jobQueue = jobQueue;
            this.jobStateReporter = jobStateReporter;
            this.jobExecuterFactories = jobExecuterFactories;
        }

        /// <inheritdoc/>
        public virtual async Task Process(CancellationToken ct = default)
        {
            var job = await jobQueue.Dequeue(null, ct);
            if (job == null)
            {
                return;
            }

            var executerFactory = jobExecuterFactories.SingleOrDefault(f => f.CanCreateFor(job));
            if (executerFactory == null)
            {
                throw new InvalidOperationException($"Could not find job executor for execution strategy {job.JobDescription.ExecutionStrategy.GetType().Name}");
            }

            var executer = await executerFactory.Create(job, ct);

            var result = await executer.Execute(job, ct);
            await jobStateReporter.Report(result, ct);
            if (!result.Success)
            {
                throw new JobExecutionException($"Execution failed: {result.Error?.Message}") { JobId = job.JobId };
            }
        }
    }
}