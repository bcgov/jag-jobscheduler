using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobScheduler.Core;
using JobScheduler.Core.Execution;

namespace JobScheduler.Providers.InMemory
{
    /// <summary>
    /// In memory implementation of job queue provider
    /// </summary>
    public class InMemoryJobQueueProvider : IJobQueueProvider
    {
        private readonly ConcurrentQueue<JobInstance> queue = new ConcurrentQueue<JobInstance>();

        /// <inheritdoc/>
        public async Task<JobInstance?> Dequeue(JobInstanceFilter? filter = null, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            return queue.TryDequeue(out var job) ? job : null;
        }

        /// <inheritdoc/>
        public async Task Enqueue(IEnumerable<JobInstance> jobs, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            foreach (var job in jobs.OrderBy(j => j.CreatedOn))
            {
                queue.Enqueue(job);
            }
        }
    }
}
