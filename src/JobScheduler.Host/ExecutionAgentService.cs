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
        private readonly int numberOfParallelAgents;

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
            this.numberOfParallelAgents = settings.ExecutionAgentSettings.NumberOfParallelAgents;
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
                bool workDone = false;
                do
                {
                    var executionTasks = Enumerable.Range(0, numberOfParallelAgents).Select(i => Execute(i, stoppingToken));
                    var executionResults = await Task.WhenAll(executionTasks);
                    workDone = Array.Exists(executionResults, r => r);
                } while (workDone);

                await Task.Delay(pollingInterval, stoppingToken);
            }

            logger.LogInformation("Stopped");
        }

        private async Task<bool> Execute(int agentId, CancellationToken ct)
        {
            bool workDone = false;
            try
            {
                logger.LogDebug("Agent {Id}: Start processing", agentId);
                workDone = await jobExecutionAgent.Process(ct);
                logger.LogDebug("Agent {Id}: End processing", agentId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Agent {Id}: Execution error: {Error}", agentId, e.Message);
                workDone = true;
            }
            return workDone;
        }
    }
}