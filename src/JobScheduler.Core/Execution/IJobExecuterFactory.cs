using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Core.Execution
{
    /// <summary>
    /// Factory of a job executer
    /// </summary>
    public interface IJobExecuterFactory
    {
        /// <summary>
        /// Create a new executer for this job instance
        /// </summary>
        /// <param name="job"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IJobExecuter> Create(JobInstance job, CancellationToken ct = default);

        /// <summary>
        /// Checks if an executer can handle a job instance
        /// </summary>
        /// <param name="job">A job instance</param>
        /// <returns>true if the executer can handle the job</returns>
        bool CanCreateFor(JobInstance job);
    }
}
