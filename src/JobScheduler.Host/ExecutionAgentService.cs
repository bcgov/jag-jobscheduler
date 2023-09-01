using System.Threading;
using System.Threading.Tasks;
using JobScheduler.Core.Execution;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobScheduler.Host
{
    /// <summary>
    /// A background service to execute queued jobs
    /// </summary>
    public class ExecutionAgentService : BackgroundService
    {
        private readonly ILogger<ExecutionAgentService> logger;
        private readonly IJobExecutionAgent jobExecutionAgent;

        /// <summary>
        /// Instantiate a new service
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="jobExecutionAgent"></param>
        public ExecutionAgentService(ILogger<ExecutionAgentService> logger, IJobExecutionAgent jobExecutionAgent)
        {
            this.logger = logger;
            this.jobExecutionAgent = jobExecutionAgent;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await jobExecutionAgent.Process(stoppingToken);
                }
                catch (System.Exception e)
                {
                    logger.LogError(e, "Execution error");
                }

                await Task.Delay(1000, stoppingToken);
            }

            logger.LogInformation("Stopped");
        }
    }
}
