using JobScheduler.Core;
using JobScheduler.Core.Configuration;
using JobScheduler.Core.JobDescriptions;
using JobScheduler.Core.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobScheduler.Providers.InMemory
{
    /// <summary>
    /// Configuration of in-memory store
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Add and configure job scheduler in-memory store
        /// </summary>
        /// <param name="builder">The Job Scheduler builder</param>
        /// <param name="jobs"></param>
        /// <returns>The Job Scheduler builder</returns>
        public static JobSchedulerConfigurationBuilder WithInMemoryStore(this JobSchedulerConfigurationBuilder builder, Func<IEnumerable<JobDescription>> jobs)
        {
            builder.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<InMemoryJobStore>>();
                var store = new InMemoryJobStore(logger);
                store.ConfigureJobs(jobs());

                return store;
            });

            builder.AddSingleton<IJobInstanceProvider>(sp => sp.GetRequiredService<InMemoryJobStore>());
            builder.AddSingleton<IJobStateReporter>(sp => sp.GetRequiredService<InMemoryJobStore>());

            return builder;
        }

        /// <summary>
        /// Add and configure in-memory job scheduler queue
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static JobSchedulerConfigurationBuilder WithInMemoryJobQueue(this JobSchedulerConfigurationBuilder builder)
        {
            builder.AddSingleton<IJobQueueProvider, InMemoryJobQueueProvider>();

            return builder;
        }
    }
}
