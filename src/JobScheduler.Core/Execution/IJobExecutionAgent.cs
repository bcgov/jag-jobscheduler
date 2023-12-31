﻿namespace JobScheduler.Core.Execution
{
    /// <summary>
    /// An interface to implement a job execution agent
    /// </summary>
    public interface IJobExecutionAgent
    {
        /// <summary>
        /// Code to start the agent's processing function
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>true if work was done, false if not    </returns>
        Task<bool> Process(CancellationToken ct = default);
    }
}