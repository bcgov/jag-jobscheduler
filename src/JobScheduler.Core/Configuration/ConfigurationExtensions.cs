using JobScheduler.Core.Dispatching;
using JobScheduler.Core.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace JobScheduler.Core.Configuration
{
    /// <summary>
    /// Initialize a job scheduler builder
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Creates the builder for job scheduling
        /// </summary>
        /// <param name="services">The initial DI services</param>
        /// <returns>A new instance of job scheduler builder</returns>
        public static JobSchedulerConfigurationBuilder AddJobScheduler(this IServiceCollection services)
        {
            var builder = JobSchedulerConfigurationBuilder.CreateFrom(services);
            builder.AddSingleton<IJobDispatcher, JobDispatcher>();
            builder.AddSingleton<IJobExecutionAgent, JobExecutionAgent>();

            return builder;
        }
    }
}
