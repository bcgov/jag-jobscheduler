using DataverseModel;
using JobScheduler.Core;
using JobScheduler.Core.Configuration;
using JobScheduler.Core.Execution;
using JobScheduler.Core.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace JobScheduler.Providers.Dynamics
{
    /// <summary>
    /// Configuration extensions for Crm store
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder">The Job Scheduler builder instance</param>
        /// <param name="configure">A configure method for Dynamics options</param>
        /// <returns></returns>
        public static JobSchedulerConfigurationBuilder RegisterCrmJobProvider(this JobSchedulerConfigurationBuilder builder, Action<IServiceProvider, CrmSettingsOptions> configure)
        {
            RegisterCrmStore(builder, configure);
            builder.AddSingleton<IJobInstanceProvider>(sp => sp.GetRequiredService<CrmStore>());
            return builder;
        }

        /// <summary>
        /// Configure Job Scheduler to use a custom action executer for Crm jobs
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static JobSchedulerConfigurationBuilder RegisterCrmCustomActionExecuter(this JobSchedulerConfigurationBuilder builder, Action<IServiceProvider, CrmSettingsOptions> configure)
        {
            RegisterCrmStore(builder, configure);

            builder.TryAddSingleton<IJobStateReporter>(sp => sp.GetRequiredService<CrmStore>());
            builder.AddTransient<IJobExecuterFactory, CustomActionJobExecuterFactory>();
            return builder;
        }

        private static void RegisterCrmStore(JobSchedulerConfigurationBuilder builder, Action<IServiceProvider, CrmSettingsOptions> configure)
        {
            builder.TryAddSingleton<IOptions<CrmSettingsOptions>>(sp =>
            {
                var opts = new CrmSettingsOptions();
                configure(sp, opts);
                return opts;
            });
            builder.TryAddSingleton<IOrganizationService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CrmSettingsOptions>>().Value!;
                if (settings.AuthenticationTokenHandler != null)
                {
                    return new ServiceClient(settings.ServiceUri, async _ => await settings.AuthenticationTokenHandler(), true, sp.GetRequiredService<ILogger<IOrganizationService>>());
                }

                throw new InvalidOperationException("Crm Store settings are not configured properly");
            });

            builder.TryAddSingleton<DataverseContext>();
            builder.TryAddSingleton<CrmStore>();
        }
    }

    /// <summary>
    /// Options to configure the Crm Store isntance
    /// </summary>
    public record CrmSettingsOptions : IOptions<CrmSettingsOptions>
    {
        /// <summary>
        /// Configures an external authentication handler
        /// </summary>
        public Func<Task<string>>? AuthenticationTokenHandler { get; set; }

        /// <summary>
        /// The url of the Dynamics instance
        /// </summary>
        public Uri ServiceUri { get; set; } = null!;

        /// <inheritdoc/>
        public CrmSettingsOptions Value => this;
    }
}
