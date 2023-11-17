using JobScheduler.Core.Execution;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobScheduler.Host
{
    /// <summary>
    /// A background service to execute queued jobs
    /// </summary>
    public class ExecutionAgentService : BackgroundService
    {
        private readonly ILogger<ExecutionAgentService> logger;
        private readonly IJobExecutionAgent jobExecutionAgent;
        private readonly JobSchedulerHostOptions settings;
        private readonly TimeSpan pollingInterval;
        private readonly TimeSpan delayedStart;

        /// <summary>
        /// Instantiate a new service
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="jobExecutionAgent"></param>
        public ExecutionAgentService(ILogger<ExecutionAgentService> logger, IOptions<JobSchedulerHostOptions> options, IJobExecutionAgent jobExecutionAgent)
        {
            ArgumentNullException.ThrowIfNull(options);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.jobExecutionAgent = jobExecutionAgent ?? throw new ArgumentNullException(nameof(jobExecutionAgent));
            this.settings = options.Value;
            this.pollingInterval = TimeSpan.FromSeconds(settings.ExecutionAgentSettings.PollingInterval);
            this.delayedStart = TimeSpan.FromSeconds(settings.ExecutionAgentSettings.DelayedStart);
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!settings.ExecutionAgentSettings.Enabled)
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
                    await jobExecutionAgent.Process(stoppingToken);
                    logger.LogDebug("End processing");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Execution error");
                }

                await Task.Delay(pollingInterval, stoppingToken);
            }

            logger.LogInformation("Stopped");
        }
    }
}
