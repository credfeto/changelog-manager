using System.Text.RegularExpressions;

namespace Credfeto.ChangeLog.Helpers;

internal static class RegexSettings
{
    public const short TimeoutMilliseconds = 1000;
    public const RegexOptions REMOVE_COMMENTS_OPTIONS = RegexOptions.Compiled | RegexOptions.Multiline;
    public const string REMOVE_COMMENTS_REGEX = "<!--[\\s\\S]*?-->";
    public const RegexOptions VERSION_HEADER_OPTIONS = RegexOptions.Compiled | RegexOptions.ExplicitCapture;
    public const string VERSION_HEADER_REGEX = @"^##\s\[(?<version>\d+(.\d+)+)\]";
    public const RegexOptions GIT_HUNK_POSITION_OPTIONS = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture;

    public const string GIT_HUNK_POSITION_REGEX =
        @"^@@\s*\-(?<OriginalFileStart>\d*)(,(?<OriginalFileEnd>\d*))?\s*\+(?<CurrentFileStart>\d*)(,(?<CurrentFileChangeLength>\d*))?\s*@@";
}