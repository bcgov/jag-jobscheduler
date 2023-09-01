using System;
using System.Threading.Tasks;
using DataverseModel;
using JobScheduler.Core;
using JobScheduler.Core.Configuration;
using JobScheduler.Core.Execution;
using JobScheduler.Core.Reporting;
using Microsoft.Extensions.DependencyInjection;
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
        /// Configures and adds Crm store to the job scheduler builder
        /// </summary>
        /// <param name="builder">The Job Scheduler builder instance</param>
        /// <param name="configure">A configure method for Dynamics options</param>
        /// <returns></returns>
        public static JobSchedulerConfigurationBuilder WithCrmStore(this JobSchedulerConfigurationBuilder builder, Action<IServiceProvider, CrmSettingsOptions> configure)
        {
            builder.AddSingleton<IOptions<CrmSettingsOptions>>(sp =>
            {
                var opts = new CrmSettingsOptions();
                configure(sp, opts);
                return opts;
            });
            builder.AddSingleton<IOrganizationService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CrmSettingsOptions>>().Value!;
                if (settings.AuthenticationTokenHandler != null)
                {
                    return new ServiceClient(settings.ServiceUri, async _ => await settings.AuthenticationTokenHandler(), false, sp.GetRequiredService<ILogger<IOrganizationService>>());
                }

                throw new InvalidOperationException("Crm Store settings are not configured properly");
            });

            builder.AddSingleton<DataverseContext>();
            builder.AddSingleton<CrmStore>();

            builder.AddSingleton<IJobInstanceProvider>(sp => sp.GetRequiredService<CrmStore>());
            builder.AddSingleton<IJobStateReporter>(sp => sp.GetRequiredService<CrmStore>());
            builder.AddTransient<IJobExecuterFactory, CustomActionJobExecuterFactory>();
            return builder;
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
