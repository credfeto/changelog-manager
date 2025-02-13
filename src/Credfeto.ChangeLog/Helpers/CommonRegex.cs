using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Helpers;

public static class CommonRegex
{
    public static readonly Regex RemoveComments = SourceGenerated.RemoveCommentsRegex();
    public static readonly Regex VersionHeader = SourceGenerated.VersionHeaderRegex();
    public static readonly Regex GitHunkPosition = SourceGenerated.GitHunkPositionRegex();
}
