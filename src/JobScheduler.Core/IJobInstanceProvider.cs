using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobScheduler.Core.Execution;

namespace JobScheduler.Core
{
    /// <summary>
    /// Provides job descriptions
    /// </summary>
    public interface IJobInstanceProvider
    {
        /// <summary>
        /// Gets all job descriptions
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<JobInstance>> GetReadyJobs(DateTimeOffset now, CancellationToken ct = default);
    }
}
