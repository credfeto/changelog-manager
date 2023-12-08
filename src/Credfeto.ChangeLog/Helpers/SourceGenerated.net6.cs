#if NET7_0_OR_GREATER
#else

using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Helpers;

internal static partial class SourceGenerated
{
    private static readonly Regex RegexRemoveComments = new(pattern: RegexSettings.RemoveCommentsRegex, options: RegexSettings.RemoveCommentsOptions, matchTimeout: Net6Compat.Short);
    private static readonly Regex RegexVersionHeader = new(pattern: RegexSettings.VersionHeaderRegex, options: RegexSettings.VersionHeaderOptions, matchTimeout: Net6Compat.Short);
    private static readonly Regex RegexGitHunkPosition = new(pattern: RegexSettings.GitHunkPositionRegex, options: RegexSettings.GitHunkPositionOptions, matchTimeout: Net6Compat.Short);

    public static Regex RemoveCommentsRegex()
    {
        return RegexRemoveComments;
    }

    public static Regex VersionHeaderRegex()
    {
        return RegexVersionHeader;
    }

    public static Regex GitHunkPositionRegex()
    {
        return RegexGitHunkPosition;
    }
}
#endif