using System;
using System.Threading;
using System.Threading.Tasks;
using JobScheduler.Core.Configuration;
using JobScheduler.Executers.Cli;
using JobScheduler.Providers.Dynamics;
using JobScheduler.Providers.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            hostBuilder.Services.AddHttpClient();
            hostBuilder.Services.AddOptions<DynamicsAuthenticationTokenProviderOptions>();
            hostBuilder.Services.Configure<DynamicsAuthenticationTokenProviderOptions>(hostBuilder.Configuration.GetSection("DynamicsAuthentication"));
            hostBuilder.Services.AddSingleton<DynamicsAutenticationTokenProvider>();
            hostBuilder.Services.AddJobScheduler()
                .WithCrmStore((sp, opts) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    opts.AuthenticationTokenHandler = sp.GetRequiredService<DynamicsAutenticationTokenProvider>().GetAccessToken;
                    opts.ServiceUri = configuration.GetValue<Uri>("DynamicsServiceUrl")!;
                })
                .WithInMemoryJobQueue()
                .RegisterCliJobExecuter()
                .AddHostedService<JobDispatcherService>()
                .AddHostedService<ExecutionAgentService>();

            var host = hostBuilder.Build();
            using var cts = new CancellationTokenSource();
            await host.RunAsync(cts.Token);
        }
    }
}