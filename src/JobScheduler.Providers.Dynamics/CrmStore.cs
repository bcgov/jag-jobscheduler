﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataverseModel;
using JobScheduler.Core;
using JobScheduler.Core.Execution;
using JobScheduler.Core.JobDescriptions;
using JobScheduler.Core.Reporting;
using Microsoft.Xrm.Sdk;

namespace JobScheduler.Providers.Dynamics
{
    /// <summary>
    /// Dynamics based implementation of job and state store
    /// </summary>
    public class CrmStore : IJobInstanceProvider, IJobStateReporter
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
                var jobDescription = new JobDescription(job.Id, job.BcGoV_Endpoint, new CustomActionExecutionStrategy(job.BcGoV_Name), new CronSchedule(CronExpression.Create("*/30 * * * * *")));
                if (job.BcGoV_LastRuntime_Date.HasValue && jobDescription.Schedule.GetNextRun(job.BcGoV_LastRuntime_Date.Value) >= now) continue;
                var sessionId = Guid.NewGuid();
                var jobInstance = new JobInstance(sessionId, jobDescription, now);
                var session = new BcGoV_ScheduleJObsession
                {
                    Id = sessionId,
                    StatusCode = BcGoV_ScheduleJObsession_StatusCode.InProgress
                };

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
            if (result is null) throw new ArgumentNullException(nameof(result));
            await Task.CompletedTask;

            var session = context.BcGoV_ScheduleJObsessionSet.Single(js => js.Id == result.JobInstanceId);
            session.StatusCode = result.Success ? BcGoV_ScheduleJObsession_StatusCode.Success : BcGoV_ScheduleJObsession_StatusCode.Failed;
            session.BcGoV_Error = result.Error?.ToString();
            context.UpdateObject(session);
            context.SaveChanges();
        }
    }
}