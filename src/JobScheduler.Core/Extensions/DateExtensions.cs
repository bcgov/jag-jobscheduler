namespace JobScheduler.Core.Extensions;

/// <summary>
/// Date related extension methods
/// </summary>
public static class DateExtensions
{
    private static TimeZoneInfo PSTTimeZone = TimeZoneInfo.FindSystemTimeZoneById(GetPSTTimeZoneId());

    private static string GetPSTTimeZoneId() => Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => "Pacific Standard Time",
        PlatformID.Unix => "America/Vancouver",
        _ => throw new NotSupportedException()
    };

    /// <summary>
    /// Returns the TimeZoneInfo of PST
    /// </summary>
    /// <returns></returns>
    public static TimeZoneInfo GetPstTimeZone() => PSTTimeZone;

    /// <summary>
    /// Converts a datetime to PST timezone
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime ToPst(this DateTime date) => TimeZoneInfo.ConvertTime(date, PSTTimeZone);
}