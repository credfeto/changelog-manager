using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Management.Helpers
{
    public static class CommonRegex
    {
        public static readonly Regex RemoveComments = new(pattern: "<!--[\\s\\S]*?-->", RegexOptions.Compiled | RegexOptions.Multiline);
        public static readonly Regex VersionHeaderMatch = new(pattern: @"^##\s\[(?<version>\d+(.\d+)+)\]", options: RegexOptions.Compiled);
    }
}