using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Helpers;

#if NET6_0
internal static class SourceGenerated
{
    public static Regex RemoveCommentsRegex()
    {
        return new(pattern: RegexSettings.RemoveCommentsRegex, options: RegexSettings.RemoveCommentsOptions, matchTimeout: Net6Compat.Short);
    }

    public static Regex VersionHeaderRegex()
    {
        return new(pattern: RegexSettings.VersionHeaderRegex, options: RegexSettings.VersionHeaderOptions, matchTimeout: Net6Compat.Short);
    }

    public static Regex GitHunkPositionRegex()
    {
        return new(pattern: RegexSettings.GitHunkPositionRegex, options: RegexSettings.GitHunkPositionOptions, matchTimeout: Net6Compat.Short);
    }
}
#else
internal static partial class SourceGenerated
{
    [GeneratedRegex(pattern: RegexSettings.RemoveCommentsRegex, options: RegexSettings.RemoveCommentsOptions, matchTimeoutMilliseconds: RegexSettings.TimeoutMilliseconds)]
    public static partial Regex RemoveCommentsRegex();

    [GeneratedRegex(pattern: RegexSettings.VersionHeaderRegex, options: RegexSettings.VersionHeaderOptions, matchTimeoutMilliseconds: RegexSettings.TimeoutMilliseconds)]
    public static partial Regex VersionHeaderRegex();

    [GeneratedRegex(pattern: RegexSettings.GitHunkPositionRegex, options: RegexSettings.GitHunkPositionOptions, matchTimeoutMilliseconds: RegexSettings.TimeoutMilliseconds)]
    public static partial Regex GitHunkPositionRegex();
}
#endif