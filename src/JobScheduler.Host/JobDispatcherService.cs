using System.Threading;
using System.Threading.Tasks;
using JobScheduler.Core.Dispatching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobScheduler.Host
{
    /// <summary>
    /// A background service to dispatch pending jobs
    /// </summary>
    public class JobDispatcherService : BackgroundService
    {
        private readonly ILogger<JobDispatcherService> logger;
        private readonly IJobDispatcher jobDispatcher;

        /// <summary>
        /// instaitiate a new service
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="jobDispatcher"></param>
        public JobDispatcherService(ILogger<JobDispatcherService> logger, IJobDispatcher jobDispatcher)
        {
            this.logger = logger;
            this.jobDispatcher = jobDispatcher;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await jobDispatcher.Dispatch(stoppingToken);
                }
                catch (System.Exception e)
                {
                    logger.LogError(e, "Dispatch error");
                }

                await Task.Delay(10000, stoppingToken);
            }

            logger.LogInformation("Stopped");
        }
    }
}