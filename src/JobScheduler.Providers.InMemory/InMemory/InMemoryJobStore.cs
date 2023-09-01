using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobScheduler.Core;
using JobScheduler.Core.Execution;
using JobScheduler.Core.JobDescriptions;
using JobScheduler.Core.Reporting;
using Microsoft.Extensions.Logging;

namespace JobScheduler.Providers.InMemory
{
    /// <summary>
    /// Stores in-memory job descriptions
    /// </summary>
    public class InMemoryJobStore : IJobInstanceProvider, IJobStateReporter
    {
        private readonly ConcurrentDictionary<Guid, Job> jobStore = new ConcurrentDictionary<Guid, Job>();
        private readonly ILogger logger;

        /// <summary>
        /// Instantiates a new in memory job store
        /// </summary>
        /// <param name="logger"></param>
        public InMemoryJobStore(ILogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<JobInstance>> GetReadyJobs(DateTimeOffset now, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return Enumerable.Empty<JobInstance>();
            }

            await Task.CompletedTask;
            var jobs = jobStore.Values.Where(j => j.IsScheduled(now)).Select(j => new JobInstance(Guid.NewGuid(), j.JobDescription, DateTimeOffset.UtcNow)).ToList();
            foreach (var job in jobs)
            {
                jobStore[job.JobDescription.JobDescriptionId].LastExecutionDate = now;
            }

            return jobs;
        }

        /// <summary>
        /// Adds a job description to the in-memory store
        /// </summary>
        public void ConfigureJobs(IEnumerable<JobDescription> jobs)
        {
            if (jobs is null) throw new ArgumentNullException(nameof(jobs));
            foreach (var job in jobs)
            {
                jobStore[job.JobDescriptionId] = new Job(job);
            }
        }

        /// <inheritdoc/>
        public async Task Report(JobExecutionResult result, CancellationToken ct = default)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            await Task.CompletedTask;
            logger.LogDebug("report: {Output}", result.Output);
            var job = jobStore.GetValueOrDefault(result.JobDescriptionId);
            if (job == null)
            {
                throw new InvalidOperationException($"Job description {result.JobDescriptionId} not found in store");
            }

            job.Results.Add(result);
            job.LastExecutionDate = DateTimeOffset.UtcNow;
        }
    }

    internal sealed record Job(JobDescription JobDescription)
    {
        public List<JobExecutionResult> Results { get; set; } = new List<JobExecutionResult>();

        public DateTimeOffset? LastExecutionDate { get; set; }

        public bool IsScheduled(DateTimeOffset referenceDateTime) => !LastExecutionDate.HasValue || JobDescription.Schedule.GetNextRun(LastExecutionDate.Value) <= referenceDateTime;
    }
}
