using JobScheduler.Core.Configuration;
using JobScheduler.Core.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace JobScheduler.Executers.Cli
{
    /// <summary>
    /// Adds and configures command-line job executer dependencies
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Add CliExecuter to DI
        /// </summary>
        /// <param name="builder">Job Scheduler builder instance</param>
        /// <returns>Job Scheduler builder instance</returns>
        public static JobSchedulerConfigurationBuilder RegisterCliJobExecuter(this JobSchedulerConfigurationBuilder builder)
        {
            builder.AddTransient<IJobExecuterFactory, CliJobExecuterFactory>();
            return builder;
        }
    }
}
