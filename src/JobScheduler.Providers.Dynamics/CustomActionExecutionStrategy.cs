using JobScheduler.Core.JobDescriptions;

namespace JobScheduler.Providers.Dynamics
{
    /// <summary>
    /// An execution strategy for a Crm custom action
    /// </summary>
    /// <param name="CustomActionName">the custom action name to invoke</param>
    public record CustomActionExecutionStrategy(string CustomActionName) : ExecutionStrategy
    {
    }
}
