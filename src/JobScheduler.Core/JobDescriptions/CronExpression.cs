using System.Diagnostics.CodeAnalysis;
using JobScheduler.Core.Extensions;

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

        private readonly Cronos.CronExpression cronSchedule;

        private CronExpression(string expression)
        {
            ArgumentNullException.ThrowIfNull(expression);
            if (expression.Split(' ').Length == 6)
            {
                cronSchedule = Cronos.CronExpression.Parse(expression, Cronos.CronFormat.IncludeSeconds);
            }
            else
            {
                cronSchedule = Cronos.CronExpression.Parse(expression, Cronos.CronFormat.Standard);
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
        public DateTimeOffset GetNextSchedule(DateTimeOffset baseSchedule) =>
            cronSchedule.GetNextOccurrence(baseSchedule, DateExtensions.GetPstTimeZone())
            ?? throw new InvalidOperationException($"No next date for {baseSchedule} in cron expression {cronSchedule.ToString()}");
    }
}