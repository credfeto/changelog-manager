using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Credfeto.ChangeLog.Management
{
    public static class ChangeLogReader
    {
        private static readonly Regex RemoveComments = new Regex(pattern: "<!--[\\s\\S]*?-->", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex VersionHeaderMatch = new Regex(pattern: @"^##\s\[(\d+)", options: RegexOptions.Compiled);

        public static async Task<string> ExtractReleaseNodesFromFileAsync(string changeLogFileName, string version)
        {
            string textBlock = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);

            return ExtractReleaseNotes(changeLog: textBlock, version: version);
        }

        public static string ExtractReleaseNotes(string changeLog, string version)
        {
            Version? releaseVersion = BuildNumberHelpers.DetermineVersionForChangeLog(version);

            string[] text = RemoveComments.Replace(input: changeLog, replacement: string.Empty)
                                          .Trim()
                                          .Split(Environment.NewLine);

            FindSectionForBuild(text: text, version: releaseVersion, out int foundStart, out int foundEnd);

            if (foundStart == -1)
            {
                return string.Empty;
            }

            if (foundEnd == -1)
            {
                foundEnd = text.Length;
            }

            string previousLine = string.Empty;

            StringBuilder releaseNotes = new StringBuilder();

            for (int i = foundStart; i < foundEnd; i++)
            {
                if (string.IsNullOrEmpty(text[i]))
                {
                    continue;
                }

                if (text[i]
                    .StartsWith(value: "### ", comparisonType: StringComparison.Ordinal) && previousLine.StartsWith(value: "### ", comparisonType: StringComparison.Ordinal))
                {
                    previousLine = text[i];

                    continue;
                }

                if (text[i]
                    .StartsWith(value: "### ", comparisonType: StringComparison.Ordinal))
                {
                    previousLine = text[i];

                    continue;
                }

                if (previousLine.StartsWith(value: "### ", comparisonType: StringComparison.Ordinal))
                {
                    releaseNotes.AppendLine(previousLine);
                }

                releaseNotes = releaseNotes.AppendLine(text[i]);
                previousLine = text[i];
            }

            return releaseNotes.ToString()
                               .Trim();
        }

        public static async Task<int?> FindFirstReleaseVersionPositionAsync(string changeLogFileName)
        {
            IReadOnlyList<string> changelog = await File.ReadAllLinesAsync(path: changeLogFileName, encoding: Encoding.UTF8);

            for (int lineIndex = 0; lineIndex < changelog.Count; ++lineIndex)
            {
                string line = changelog[lineIndex];

                if (VersionHeaderMatch.IsMatch(line))
                {
                    return lineIndex;
                }
            }

            return null;
        }

        private static void FindSectionForBuild(string[] text, Version? version, out int foundStart, out int foundEnd)
        {
            foundStart = -1;
            foundEnd = -1;

            for (int i = 1; i < text.Length; i++)
            {
                string line = text[i];

                if (IsMatchingVersion(version: version, line: line))
                {
                    foundStart = i + 1;

                    continue;
                }

                if (foundStart != -1 && line.StartsWith(value: "## [", comparisonType: StringComparison.Ordinal))
                {
                    foundEnd = i;

                    break;
                }
            }
        }

        private static bool IsMatchingVersion(Version? version, string line)
        {
            if (version == null)
            {
                return StringComparer.InvariantCultureIgnoreCase.Equals(x: line, y: "## [Unreleased]");
            }

            static IEnumerable<string> Candidates(Version expected)
            {
                int build = expected.Build == 0 || expected.Build == -1 ? 0 : expected.Build;

                yield return $"## [{expected.Major}.{expected.Minor}.{build}]";

                if (build == 0)
                {
                    yield return $"## [{expected.Major}.{expected.Minor}]";
                }
            }

            foreach (var candidate in Candidates(version))
            {
                if (line.StartsWith(value: candidate, comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}