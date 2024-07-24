using System.Diagnostics.CodeAnalysis;

namespace JobScheduler.Core.Extensions;

/// <summary>
/// String extension methods
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Extract a string from another string without exceesing the lentgh
    /// </summary>
    /// <param name="s"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string SafeSubstring([NotNull] this string s, int length) => s[..(s.Length >= length ? length : s.Length)];
}