using JobScheduler.Core;
using JobScheduler.Core.Execution;
using JobScheduler.Core.Extensions;
using JobScheduler.Core.JobDescriptions;
using JobScheduler.Core.Reporting;
using JobScheduler.Providers.Dynamics.Model;
using Microsoft.Xrm.Sdk;

namespace JobScheduler.Providers.Dynamics
{
    /// <summary>
    /// Dynamics based implementation of job and state store
    /// </summary>
    internal sealed class CrmStore : IJobInstanceProvider, IJobStateReporter, IJobQueueProvider
    {
        private readonly DataverseContext context;

        /// <summary>
        /// Instantiates a new instance of crm store
        /// </summary>
        /// <param name="context"></param>
        public CrmStore(DataverseContext context)
        {
            this.context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<JobInstance>> GetReadyJobs(DateTimeOffset now, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            var jobs = context.BcGoV_ScheduleJobSet
                .Where(sj => sj.StateCode == BcGoV_ScheduleJob_StateCode.Active && (sj.BcGoV_NextRuntime == null || sj.BcGoV_NextRuntime <= now.UtcDateTime))
                .ToList();
            var jobInstances = new List<JobInstance>();
            foreach (var job in jobs)
            {
                var isFirstTimeJob = job.BcGoV_NextRuntime == null;
                var jobDescription = new JobDescription(
                    job.BcGoV_ScheduleJobId!.Value,
                    job.BcGoV_Name,
                    new CustomActionExecutionStrategy(job.BcGoV_Endpoint),
                    new CronSchedule(CronExpression.Create(job.BcGoV_CroneXpResSiOn)));

                job.BcGoV_NextRuntime = DateTime.SpecifyKind(jobDescription.Schedule.GetNextRun(now).DateTime, DateTimeKind.Local);
                context.UpdateObject(job);

                if (!isFirstTimeJob)
                {
                    var sessionId = Guid.NewGuid();
                    var jobInstance = new JobInstance(sessionId, jobDescription, now);
                    jobInstances.Add(jobInstance);
                }
            }

            context.SaveChanges();

            return jobInstances;
        }

        /// <inheritdoc/>
        public async Task Report(JobExecutionResult result, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(result);
            await Task.CompletedTask;

            var session = context.BcGoV_ScheduleJObsessionSet.Single(js => js.Id == result.JobInstanceId);
            session.StatusCode = result.Success ? BcGoV_ScheduleJObsession_StatusCode.Success : BcGoV_ScheduleJObsession_StatusCode.Failed;
            session.StateCode = result.Success ? BcGoV_ScheduleJObsession_StateCode.Inactive : BcGoV_ScheduleJObsession_StateCode.Active;
            session.BcGoV_Error = result.Error?.ToString().SafeSubstring(4000);
            context.UpdateObject(session);
            context.SaveChanges();
        }

        public async Task<JobInstance?> Dequeue(JobInstanceFilter? filter = null, CancellationToken ct = default)
        {
            await Task.CompletedTask;

            var result = context.BcGoV_ScheduleJObsessionSet
                .Join(context.BcGoV_ScheduleJobSet, s => s.BcGoV_ScheduleJobId.Id, j => j.BcGoV_ScheduleJobId, (s, j) => new { s, j })
                .Where(r => r.s.StatusCode == BcGoV_ScheduleJObsession_StatusCode.InProgress)
                .OrderBy(r => r.s.CreatedOn)
                .FirstOrDefault();
            if (result == null) return null;
            var session = result.s;
            var job = result.j;
            var jobDescription = new JobDescription(job.BcGoV_ScheduleJobId!.Value, job.BcGoV_Name, new CustomActionExecutionStrategy(job.BcGoV_Endpoint), new CronSchedule(CronExpression.Create(job.BcGoV_CroneXpResSiOn)));

            return new JobInstance(session.Id, jobDescription, (DateTimeOffset)session.CreatedOn!);
        }

        public async Task Enqueue(IEnumerable<JobInstance> jobs, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            foreach (var jobInstance in jobs)
            {
                var job = context.BcGoV_ScheduleJobSet.FirstOrDefault(j => j.BcGoV_ScheduleJobId == jobInstance.JobDescription.JobDescriptionId);
                if (job == null) continue;
                var session = new BcGoV_ScheduleJObsession
                {
                    Id = jobInstance.JobId,
                    StatusCode = BcGoV_ScheduleJObsession_StatusCode.InProgress,
                    BcGoV_ScheduleJobId = new EntityReference("bcgov_schedulejob", job.Id)
                };
                context.AddObject(session);
                //context.AddLink(session, new Relationship(BcGoV_ScheduleJObsession.Fields.BcGoV_ScheduleJob_BcGoV_ScheduleJObsession), job);
            }
            context.SaveChanges();
        }
    }
}