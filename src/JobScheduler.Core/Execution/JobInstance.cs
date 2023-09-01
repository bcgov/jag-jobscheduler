using System;
using JobScheduler.Core.JobDescriptions;

namespace JobScheduler.Core.Execution
{
    /// <summary>
    /// Represents a job instance to run
    /// </summary>
    /// <param name="JobId">The job id</param>
    /// <param name="JobDescription">The job descrption</param>
    /// <param name="CreatedOn">The date the job instance created</param>
    public record JobInstance(Guid JobId, JobDescription JobDescription, DateTimeOffset CreatedOn)
    {
    }
}
