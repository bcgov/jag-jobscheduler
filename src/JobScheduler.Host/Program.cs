using JobScheduler.Core.Configuration;
using JobScheduler.Executers.Cli;
using JobScheduler.Providers.Dynamics;
using JobScheduler.Providers.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace JobScheduler.Host
{
    /// <summary>
    /// Job Scheduler host
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main program
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

            hostBuilder.Services.AddSerilog(config =>
            {
                config.ReadFrom.Configuration(hostBuilder.Configuration);
            });

            hostBuilder.Services.AddHttpClient();

            hostBuilder.Services.AddOptions<JobSchedulerHostOptions>();
            hostBuilder.Services.Configure<JobSchedulerHostOptions>(hostBuilder.Configuration.GetSection("Host"));

            hostBuilder.Services.AddOptions<DynamicsAuthenticationTokenProviderOptions>();
            hostBuilder.Services.Configure<DynamicsAuthenticationTokenProviderOptions>(hostBuilder.Configuration.GetSection("DynamicsAuthentication"));

            hostBuilder.Services.AddSingleton<DynamicsAutenticationTokenProvider>();

            hostBuilder.Services.AddJobScheduler()
                .RegisterCrmJobProvider(ConfigureCrmOptions).WithInMemoryJobQueue()
                .RegisterCliJobExecuter()
                .RegisterCrmCustomActionExecuter(ConfigureCrmOptions)
                .AddHostedService<JobDispatcherService>()
                .AddHostedService<ExecutionAgentService>();

            var host = hostBuilder.Build();
            using var cts = new CancellationTokenSource();

            await host.RunAsync(cts.Token);
        }

        private static void ConfigureCrmOptions(IServiceProvider serviceProvider, CrmSettingsOptions options)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            options.AuthenticationTokenHandler = serviceProvider.GetRequiredService<DynamicsAutenticationTokenProvider>().GetAccessToken;
            options.ServiceUri = configuration.GetValue<Uri>("DynamicsServiceUrl")!;
        }
    }
}