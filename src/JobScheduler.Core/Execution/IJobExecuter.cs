using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Core.Execution
{
    /// <summary>
    /// Job executer interface
    /// </summary>
    public interface IJobExecuter
    {
        /// <summary>
        ///Executes a job
        /// </summary>
        /// <param name="job">The job instance to execute</param>
        /// <param name="ct">Optional cancellation token</param>
        /// <returns>An execution result</returns>
        Task<JobExecutionResult> Execute(JobInstance job, CancellationToken ct = default);
    }
}
