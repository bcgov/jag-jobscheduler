using System;
using System.Threading;
using System.Threading.Tasks;
using DataverseModel;
using JobScheduler.Core.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace JobScheduler.Providers.Dynamics
{
    /// <summary>
    /// Factory for Dynamic custom action job executer
    /// </summary>
    public class CustomActionJobExecuterFactory : IJobExecuterFactory
    {
        private readonly IServiceProvider services;

        /// <summary>
        /// Instantiate a new factory
        /// </summary>
        /// <param name="services"></param>
        public CustomActionJobExecuterFactory(IServiceProvider services)
        {
            this.services = services;
        }

        /// <inheritdoc/>
        public bool CanCreateFor(JobInstance job)
        {
            if (job is null) throw new ArgumentNullException(nameof(job));
            return job.JobDescription.ExecutionStrategy is CustomActionExecutionStrategy;
        }

        /// <inheritdoc/>
        public async Task<IJobExecuter> Create(JobInstance job, CancellationToken ct = default)
        {
            if (job is null) throw new ArgumentNullException(nameof(job));
            var customActionName = ((CustomActionExecutionStrategy)job.JobDescription.ExecutionStrategy).CustomActionName;
            return await Task.FromResult(new CustomActionJobExecuter(services.GetRequiredService<DataverseContext>(), customActionName));
        }
    }
}
