using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Helpers;

#if NET6_0
internal static class SourceGenerated
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(RegexSettings.TimeoutMilliseconds);

    public static Regex RemoveCommentsRegex()
    {
        return new(pattern: RegexSettings.REMOVE_COMMENTS_REGEX, options: RegexSettings.REMOVE_COMMENTS_OPTIONS, matchTimeout: Short);
    }

    public static Regex VersionHeaderRegex()
    {
        return new(pattern: RegexSettings.VERSION_HEADER_REGEX, options: RegexSettings.VERSION_HEADER_OPTIONS, matchTimeout: Short);
    }

    public static Regex GitHunkPositionRegex()
    {
        return new(pattern: RegexSettings.GIT_HUNK_POSITION_REGEX, options: RegexSettings.GIT_HUNK_POSITION_OPTIONS, matchTimeout: Short);
    }
}
#else
internal static partial class SourceGenerated
{
    [GeneratedRegex(pattern: RegexSettings.REMOVE_COMMENTS_REGEX, options: RegexSettings.REMOVE_COMMENTS_OPTIONS, matchTimeoutMilliseconds: RegexSettings.TimeoutMilliseconds)]
    public static partial Regex RemoveCommentsRegex();

    [GeneratedRegex(pattern: RegexSettings.VERSION_HEADER_REGEX, options: RegexSettings.VERSION_HEADER_OPTIONS, matchTimeoutMilliseconds: RegexSettings.TimeoutMilliseconds)]
    public static partial Regex VersionHeaderRegex();

    [GeneratedRegex(pattern: RegexSettings.GIT_HUNK_POSITION_REGEX, options: RegexSettings.GIT_HUNK_POSITION_OPTIONS, matchTimeoutMilliseconds: RegexSettings.TimeoutMilliseconds)]
    public static partial Regex GitHunkPositionRegex();
}
#endif