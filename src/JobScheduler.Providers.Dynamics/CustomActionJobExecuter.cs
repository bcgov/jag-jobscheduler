using JobScheduler.Core.Execution;
using JobScheduler.Providers.Dynamics.Model;
using Microsoft.Xrm.Sdk;

namespace JobScheduler.Providers.Dynamics
{
    /// <summary>
    /// A Crm custom action executer instance
    /// </summary>
    internal sealed class CustomActionJobExecuter : IJobExecuter
    {
        private readonly DataverseContext context;
        private readonly string customActionName;

        /// <summary>
        /// Creates a new instance of Crm custom action job executer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="customActionName"></param>
        public CustomActionJobExecuter(DataverseContext context, string customActionName)
        {
            this.context = context;
            this.customActionName = customActionName;
        }

        /// <inheritdoc/>
        public async Task<JobExecutionResult> Execute(JobInstance job, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            try
            {
                var request = new OrganizationRequest(customActionName);
                var response = context.Execute(request);

                return new JobExecutionResult(job.JobDescription.JobDescriptionId, job.JobId, true, null, response.ResponseName);
            }
            catch (Exception e)
            {
                return new JobExecutionResult(job.JobDescription.JobDescriptionId, job.JobId, false, e, string.Empty);
            }
        }
    }
}
