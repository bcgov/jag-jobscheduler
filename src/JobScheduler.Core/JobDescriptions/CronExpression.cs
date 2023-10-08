using System.Diagnostics.CodeAnalysis;
using NCrontab;

namespace JobScheduler.Core.JobDescriptions
{
    /// <summary>
    /// A crontab compabible expression
    /// </summary>
    public readonly struct CronExpression : IEquatable<CronExpression>
    {
        /// <summary>
        /// Factory method to create a cron expression from a string
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <returns></returns>
        public static CronExpression Create(string cronExpression) => new CronExpression(cronExpression);

        private readonly CrontabSchedule cronSchedule;

        private CronExpression(string expression)
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));
            if (expression.Split(' ').Length == 6)
            {
                cronSchedule = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            }
            else
            {
                cronSchedule = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = false });
            }
        }

        /// <inheritdoc/>

        public override string ToString() => cronSchedule.ToString();

        /// <inheritdoc/>

        public override int GetHashCode() => cronSchedule.ToString().GetHashCode(StringComparison.Ordinal);

        /// <inheritdoc/>

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is CronExpression e && Equals(e);

        /// <inheritdoc/>

        public bool Equals(CronExpression other) => cronSchedule.ToString().Equals(other.cronSchedule.ToString(), StringComparison.Ordinal);

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(CronExpression left, CronExpression right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(CronExpression left, CronExpression right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets the next schedule for this cron expression relative to a base point in time
        /// </summary>
        /// <param name="baseSchedule"></param>
        /// <returns></returns>
        public DateTime GetNextSchedule(DateTime baseSchedule) => cronSchedule.GetNextOccurrence(baseSchedule);
    }
}
