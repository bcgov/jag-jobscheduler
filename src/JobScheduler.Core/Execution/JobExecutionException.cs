namespace JobScheduler.Core.Execution;

/// <summary>
/// Represents an exception within a job execution flow
/// </summary>
[System.Serializable]
public class JobExecutionException : Exception
{
    /// <summary>
    /// The job id of that caused the exception
    /// </summary>
    public Guid? JobId
    {
        get => (Guid?)Data["JobId"];
        set => Data["JobId"] = value;
    }

    /// <summary>
    /// empty construstor
    /// </summary>
    public JobExecutionException()
    { }

    /// <summary>
    /// constrcutor with a message
    /// </summary>
    /// <param name="message">the message</param>
    public JobExecutionException(string message) : base(message)
    {
    }

    /// <summary>
    /// constrcutor with a message and an inner exception
    /// </summary>
    /// <param name="message">the message</param>
    /// <param name="inner">the inner exception</param>
    public JobExecutionException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>
    /// serialization constructor
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected JobExecutionException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
