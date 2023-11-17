using JobScheduler.Core.Dispatching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobScheduler.Host
{
    /// <summary>
    /// A background service to dispatch pending jobs
    /// </summary>
    public class JobDispatcherService : BackgroundService
    {
        private readonly ILogger<JobDispatcherService> logger;
        private readonly IJobDispatcher jobDispatcher;
        private readonly JobSchedulerHostOptions settings;
        private readonly TimeSpan pollingInterval;
        private readonly TimeSpan delayedStart;

        /// <summary>
        /// instaitiate a new service
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="jobDispatcher"></param>
        public JobDispatcherService(ILogger<JobDispatcherService> logger, IOptions<JobSchedulerHostOptions> options, IJobDispatcher jobDispatcher)
        {
            ArgumentNullException.ThrowIfNull(options);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.jobDispatcher = jobDispatcher ?? throw new ArgumentNullException(nameof(jobDispatcher));
            this.settings = options.Value;
            this.pollingInterval = TimeSpan.FromSeconds(settings.DispatcherSettings.PollingInterval);
            this.delayedStart = TimeSpan.FromSeconds(settings.DispatcherSettings.DelayedStart);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!settings.DispatcherSettings.Enabled)
            {
                logger.LogWarning("Disabled");
                return;
            }

            logger.LogInformation("About to start");
            await Task.Delay(delayedStart, stoppingToken);
            logger.LogInformation("Started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogDebug("Start processing");
                    await jobDispatcher.Dispatch(stoppingToken);
                    logger.LogDebug("End processing");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Dispatch error");
                }

                await Task.Delay(pollingInterval, stoppingToken);
            }

            logger.LogInformation("Stopped");
        }
    }
}