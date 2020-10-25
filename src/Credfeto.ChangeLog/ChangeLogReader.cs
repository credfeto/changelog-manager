using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Credfeto.ChangeLog.Management
{
    public sealed class ChangeLogReader
    {
        private static readonly Regex RemoveComments = new Regex(pattern: "(?ms)<!--(.*)-->", options: RegexOptions.Compiled);

        public static async Task<string> ExtractReleasNodesFromFileAsync(string changeLogFileName, string version)
        {
            string textBlock = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);

            return ExtractReleaseNotes(changeLog: textBlock, version: version);
        }

        public static string ExtractReleaseNotes(string changeLog, string version)
        {
            string buildNumber = BuildNumberHelpers.DetermineBuildNumberForChangeLog(version);
            string[] text = changeLog.Split(Environment.NewLine);

            FindSectionForBuild(text: text, buildNumber: buildNumber, out int foundStart, out int foundEnd);

            StringBuilder releaseNotes = new StringBuilder();

            if (foundStart == -1)
            {
                return string.Empty;
            }

            if (foundEnd == -1)
            {
                foundEnd = text.Length;
            }

            string previousLine = "";

            for (int i = foundStart; i < foundEnd; i++)
            {
                if (text[i] == "")
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

            if (buildNumber == "unreleased")
            {
                return RemoveComments.Replace(releaseNotes.ToString(), replacement: string.Empty)
                                     .Trim();
            }

            return releaseNotes.ToString()
                               .Trim();
        }

        private static void FindSectionForBuild(string[] text, string buildNumber, out int foundStart, out int foundEnd)
        {
            foundStart = -1;
            foundEnd = -1;

            for (int i = 1; i < text.Length; i++)
            {
                if (text[i]
                    .StartsWith("## [" + buildNumber, comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    foundStart = i + 1;

                    continue;
                }

                if (foundStart != -1 && text[i]
                    .StartsWith(value: "## [", comparisonType: StringComparison.Ordinal))
                {
                    foundEnd = i;

                    break;
                }
            }
        }
    }
}