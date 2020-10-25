using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Credfeto.ChangeLog.Management
{
    public sealed class ChangeLogUpdater
    {
        private static readonly Regex RemoveComments = new Regex(pattern: "(?ms)<!--(.*)-->", options: RegexOptions.Compiled);

        public static async Task<string> ReadAsync(string changeLogFileName, string version)
        {
            string textBlock = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);

            string buildNumber = BuildNumberHelpers.DetermineBuildNumerForChangeLog(version);
            string[] text = textBlock.Split(Environment.NewLine);

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

        public static async Task AddEntryAsync(string changeLogFileName, string type, string message)
        {
            string textBlock;

            if (!File.Exists(changeLogFileName))
            {
                await CreateEmptyAsync(changeLogFileName);
                textBlock = TemplateFile.Initial;
            }
            else
            {
                textBlock = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);
            }

            string[] text = textBlock.Split(Environment.NewLine);

            StringBuilder output = new StringBuilder();
            bool foundUnreleased = false;
            bool done = false;

            foreach (var candidateLine in text)
            {
                string line = candidateLine.TrimEnd();
                output.AppendLine(line);

                if (done)
                {
                    continue;
                }

                if (!foundUnreleased)
                {
                    if (line == "## [Unreleased]")
                    {
                        foundUnreleased = true;
                    }
                }
                else
                {
                    if (line == "### " + type)
                    {
                        //Write-Information "* Changelog Insert position added"
                        output.AppendLine("- " + message);
                        done = true;
                    }
                }
            }

            if (!done)
            {
                throw new InvalidChangeLogException("Could not find [Unreleased] section of file");
            }

            //Write-Information "* Saving Changelog"
            await File.WriteAllTextAsync(path: changeLogFileName,
                                         output.ToString()
                                               .Trim(),
                                         encoding: Encoding.UTF8);
        }

        public static Task CreateEmptyAsync(string changeLogFileName)
        {
            return File.WriteAllTextAsync(path: changeLogFileName, contents: TemplateFile.Initial, encoding: Encoding.UTF8);
        }
    }

    internal static class BuildNumberHelpers
    {
        public static string DetermineBuildNumerForChangeLog(string version)
        {
            string buildNumber;

            if (version.Contains('-'))
            {
                buildNumber = "unreleased";
            }
            else
            {
                buildNumber = version.Substring(startIndex: 0, version.LastIndexOf('.'));
            }

            return buildNumber;
        }
    }
}