using JobScheduler.Core;
using JobScheduler.Core.Execution;
using JobScheduler.Core.JobDescriptions;
using JobScheduler.Core.Reporting;
using JobScheduler.Providers.Dynamics.Model;
using Microsoft.Xrm.Sdk;

namespace JobScheduler.Providers.Dynamics
{
    /// <summary>
    /// Dynamics based implementation of job and state store
    /// </summary>
    internal sealed class CrmStore : IJobInstanceProvider, IJobStateReporter
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
            var jobs = context.BcGoV_ScheduleJobSet.Where(sj => sj.StateCode == BcGoV_ScheduleJob_StateCode.Active).ToList();
            var jobInstances = new List<JobInstance>();
            foreach (var job in jobs)
            {
                var jobDescription = new JobDescription(job.Id, job.BcGoV_Endpoint, new CustomActionExecutionStrategy(job.BcGoV_Name), new CronSchedule(CronExpression.Create(job.BcGoV_CroneXpResSiOn)));
                if (job.BcGoV_LastRuntime_Date.HasValue && job.BcGoV_NextRuntime > now) continue;
                var sessionId = Guid.NewGuid();
                var jobInstance = new JobInstance(sessionId, jobDescription, now);
                var session = new BcGoV_ScheduleJObsession
                {
                    Id = sessionId,
                    StatusCode = BcGoV_ScheduleJObsession_StatusCode.InProgress
                };

                job.BcGoV_NextRuntime = jobDescription.Schedule.GetNextRun(now).DateTime;

                context.UpdateObject(job);
                context.AddObject(session);
                context.AddLink(session, new Relationship(BcGoV_ScheduleJObsession.Fields.BcGoV_ScheduleJob_BcGoV_ScheduleJObsession), job);
                jobInstances.Add(jobInstance);
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
            session.BcGoV_Error = result.Error?.ToString();
            context.UpdateObject(session);
            context.SaveChanges();
        }
    }
}