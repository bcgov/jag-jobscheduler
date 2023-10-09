using JobScheduler.Core.Execution;

namespace JobScheduler.Executers.Cli
{
    /// <summary>
    /// Factory class for cli job executers
    /// </summary>
    public class CliJobExecuterFactory : IJobExecuterFactory
    {
        /// <inheritdoc/>
        public bool CanCreateFor(JobInstance job)
        {
            if (job is null) throw new ArgumentNullException(nameof(job));
            return job.JobDescription.ExecutionStrategy is CliExecutionStrategy;
        }

        /// <inheritdoc/>
        public async Task<IJobExecuter> Create(JobInstance job, CancellationToken ct = default)
        {
            if (job is null) throw new ArgumentNullException(nameof(job));
            return CanCreateFor(job)
                ? await Task.FromResult(new CliJobExecuter())
                : throw new InvalidOperationException($"Job execution strategy {job.JobDescription.ExecutionStrategy.GetType().Name} cannot be handled by {nameof(CliJobExecuter)}");
        }
    }
}
