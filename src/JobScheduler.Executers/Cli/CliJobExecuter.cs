using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Instances;
using JobScheduler.Core.Execution;

namespace JobScheduler.Executers.Cli
{
    /// <summary>
    /// Command line executer that
    /// </summary>
    public class CliJobExecuter : IJobExecuter
    {
        /// <inheritdoc/>
        public async Task<JobExecutionResult> Execute(JobInstance job, CancellationToken ct = default)
        {
            if (job is null) throw new ArgumentNullException(nameof(job));
            if (job.JobDescription.ExecutionStrategy is not CliExecutionStrategy strategy)
            {
                throw new InvalidOperationException($"Unexpected job execution strategy {job.JobDescription.ExecutionStrategy.GetType().Name}");
            }

            var output = new StringBuilder();

            try
            {
                var processArgument = new ProcessArguments(strategy.Command, strategy.Arguments);
                processArgument.OutputDataReceived += (sender, data) => output.AppendLine(data);
                processArgument.ErrorDataReceived += (sender, data) => output.AppendLine(data);
                IProcessResult? result = null;
                processArgument.Exited += (sender, data) => result = data;
                using var processInfo = processArgument.Start();
#pragma warning disable CA1508 // Avoid dead conditional code
                while (!ct.IsCancellationRequested && result == null)
                {
                    await Task.Delay(1000, ct);
                }

                var exitCode = result?.ExitCode ?? -1;
                return new JobExecutionResult(job.JobDescription.JobDescriptionId, job.JobId, exitCode == strategy.SuccessExitCode, null, output.ToString());
#pragma warning restore CA1508 // Avoid dead conditional code
            }
            catch (Exception e)
            {
                return new JobExecutionResult(job.JobDescription.JobDescriptionId, job.JobId, false, e, output.ToString());
            }
        }
    }
}
