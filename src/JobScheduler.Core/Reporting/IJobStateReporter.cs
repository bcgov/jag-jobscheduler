using JobScheduler.Core.Execution;

namespace JobScheduler.Core.Reporting
{
    /// <summary>
    /// Collects job execution results
    /// </summary>
    public interface IJobStateReporter
    {
        /// <summary>
        /// Report the result of a job instance execution
        /// </summary>
        /// <param name="result">The results</param>
        /// <param name="ct">A cancellation token</param>
        /// <returns></returns>
        Task Report(JobExecutionResult result, CancellationToken ct = default);
    }
}
