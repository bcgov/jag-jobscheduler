using System.Threading;
using System.Threading.Tasks;

namespace JobScheduler.Core.Execution
{
    /// <summary>
    /// An interface to implement a job execution agent
    /// </summary>
    public interface IJobExecutionAgent
    {
        /// <summary>
        /// Code to start the agent's processing function
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task Process(CancellationToken ct = default);
    }
}