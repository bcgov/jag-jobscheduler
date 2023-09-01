using JobScheduler.Core.JobDescriptions;

namespace JobScheduler.Executers.Cli
{
    /// <summary>
    /// Represents a command line job execution strategy
    /// </summary>
    public record CliExecutionStrategy(string Command, string Arguments, int SuccessExitCode = 0) : ExecutionStrategy;
}
