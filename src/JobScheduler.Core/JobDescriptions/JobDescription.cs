namespace JobScheduler.Core.JobDescriptions
{
    /// <summary>
    /// Represents a job description metadata to be executed by the job scheduler
    /// </summary>
    /// <param name="JobDescriptionId">The job description unique id</param>
    /// <param name="Description">A human friendly job description text</param>
    /// <param name="ExecutionStrategy">An execution strategy that will be used to execute the job</param>
    /// <param name="Schedule"></param>
    public record JobDescription(Guid JobDescriptionId, string Description, ExecutionStrategy ExecutionStrategy, JobSchedule Schedule)
    {
    }

    /// <summary>
    /// Abstract base record for execution strategy types
    /// </summary>
    public abstract record ExecutionStrategy();

    /// <summary>
    /// Abstract base record for job schedule types
    /// </summary>
    public abstract record JobSchedule()
    {
        /// <summary>
        /// Calculates the absolute date of the next scheduled run
        /// </summary>
        /// <param name="lastRun">The last execution time</param>
        /// <returns>The next scheduled time </returns>
        public abstract DateTimeOffset GetNextRun(DateTimeOffset lastRun);
    }

    /// <summary>
    /// Represents a job schedule based on CronTab expression
    /// </summary>
    /// <param name="CronExpression"></param>
    public record CronSchedule(CronExpression CronExpression) : JobSchedule
    {
        /// <inheritdoc/>
        public override DateTimeOffset GetNextRun(DateTimeOffset lastRun) => CronExpression.GetNextSchedule(lastRun);
    }

    /// <summary>
    /// Represents a schedule to run immediately
    /// </summary>
    public partial record AdHocSchedule() : JobSchedule
    {
        /// <inheritdoc/>
        public override DateTimeOffset GetNextRun(DateTimeOffset lastRun) => DateTimeOffset.UtcNow;
    }
}