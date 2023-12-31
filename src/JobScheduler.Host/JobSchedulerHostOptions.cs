﻿using Microsoft.Extensions.Options;

namespace JobScheduler.Host;

/// <summary>
/// Options to configure the job scheduler host
/// </summary>
public record JobSchedulerHostOptions : IOptions<JobSchedulerHostOptions>
{
    /// <summary>
    /// Adds an additional configuration file to support json environment configuration at runtime.
    /// </summary>
    public string? AdditionalConfigurationFile { get; set; }

    /// <summary>
    /// Dispatcher related settings
    /// </summary>
    public DispatcherSettings DispatcherSettings { get; set; } = new DispatcherSettings();

    /// <summary>
    /// Execution agent related settings
    /// </summary>
    public ExecutionAgentSettings ExecutionAgentSettings { get; set; } = new ExecutionAgentSettings();

    /// <inheritdoc/>
    public JobSchedulerHostOptions Value => this;
}

/// <summary>
/// Execution agent settings
/// </summary>
public record ExecutionAgentSettings
{
    /// <summary>
    /// Configures the execution agent on or off
    /// </summary>
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// Polling interval to process pending jobs
    /// </summary>
    public int PollingInterval { get; set; } = 15;

    /// <summary>
    /// Delayed start for polling in seconds
    /// </summary>
    public int DelayedStart { get; set; } = 30;

    /// <summary>
    /// The number of parallel agents to host, defaults to 1
    /// </summary>
    public int NumberOfParallelAgents { get; set; } = 5;
}

/// <summary>
/// Dispatcher settings
/// </summary>
public record DispatcherSettings
{
    /// <summary>
    /// Configures the dispatcher on or off
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Polling interval in seconds to dispatch pending jobs
    /// </summary>
    public int PollingInterval { get; set; } = 30;

    /// <summary>
    /// Delayed start for polling in seconds
    /// </summary>
    public int DelayedStart { get; set; } = 10;
}
