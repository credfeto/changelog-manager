using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Helpers
{
    public static class CommonRegex
    {
        public static readonly Regex RemoveComments = new(pattern: "<!--[\\s\\S]*?-->", RegexOptions.Compiled | RegexOptions.Multiline, matchTimeout: RegexTimeouts.Short);
        public static readonly Regex VersionHeaderMatch = new(pattern: @"^##\s\[(?<version>\d+(.\d+)+)\]", RegexOptions.Compiled | RegexOptions.ExplicitCapture, matchTimeout: RegexTimeouts.Short);
    }
}