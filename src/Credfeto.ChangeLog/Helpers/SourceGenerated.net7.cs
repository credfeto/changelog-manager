#if NET7_0_OR_GREATER
using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Helpers;

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