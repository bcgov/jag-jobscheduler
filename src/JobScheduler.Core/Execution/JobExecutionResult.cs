using System;

namespace JobScheduler.Core.Execution
{
    /// <summary>
    /// Represents a job execution result
    /// </summary>
    public record JobExecutionResult(Guid JobDescriptionId, Guid JobInstanceId, bool Success, Exception? Error, string Output);
}
