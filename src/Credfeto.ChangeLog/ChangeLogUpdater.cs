using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Credfeto.ChangeLog.Management
{
    public static class ChangeLogUpdater
    {
        public static async Task AddEntryAsync(string changeLogFileName, string type, string message)
        {
            string textBlock = await ReadChangeLogAsync(changeLogFileName);

            string content = AddEntry(changeLog: textBlock, type: type, message: message);

            await File.WriteAllTextAsync(path: changeLogFileName, contents: content, encoding: Encoding.UTF8);
        }

        private static async Task<string> ReadChangeLogAsync(string changeLogFileName)
        {
            if (!File.Exists(changeLogFileName))
            {
                await CreateEmptyAsync(changeLogFileName);

                return TemplateFile.Initial;
            }

            return await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);
        }

        public static string AddEntry(string changeLog, string type, string message)
        {
            List<string> text = EnsureChangelog(changeLog)
                                .Split(Environment.NewLine)
                                .ToList();

            int index = FindInsertPosition(changeLog: text, type: type);
            text.Insert(index: index, "- " + message);

            string content = string.Join(separator: Environment.NewLine, values: text)
                                   .Trim();

            return content;
        }

        private static int FindInsertPosition(List<string> changeLog, string type)
        {
            bool foundUnreleased = false;

            string search = "### " + type;

            for (int index = 0; index < changeLog.Count; index++)
            {
                string? line = changeLog[index];

                if (!foundUnreleased)
                {
                    if (line == "## [Unreleased]")
                    {
                        foundUnreleased = true;
                    }
                }
                else
                {
                    if (line == search)
                    {
                        int next = index + 1;

                        while (next < changeLog.Count)
                        {
                            if (IsNextItem(changeLog[next]))
                            {
                                return FindPreviousNonBlankEntry(changeLog: changeLog, earliest: index, latest: next);
                            }

                            ++next;
                        }

                        return index + 1;
                    }
                }
            }

            throw new InvalidChangeLogException("Could not find [Unreleased] section of file");
        }

        public static async Task CreateReleaseAsync(string changeLogFileName, string version)
        {
            string originalChangeLog = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);

            string newChangeLog = CreateRelease(changeLog: originalChangeLog, version: version);

            await File.WriteAllTextAsync(path: changeLogFileName, contents: newChangeLog, encoding: Encoding.UTF8);
        }

        public static string CreateRelease(string changeLog, string version)
        {
            return changeLog + version;
        }

        private static int FindPreviousNonBlankEntry(List<string> changeLog, int earliest, int latest)
        {
            int previous = latest - 1;

            while (previous > earliest && string.IsNullOrWhiteSpace(changeLog[previous]))
            {
                --previous;

                if (!string.IsNullOrWhiteSpace(changeLog[previous]))
                {
                    return previous + 1;
                }
            }

            return latest;
        }

        private static bool IsNextItem(string line)
        {
            return line.StartsWith(value: "#", comparisonType: StringComparison.Ordinal) || line.StartsWith(value: "<!--", comparisonType: StringComparison.Ordinal);
        }

        private static string EnsureChangelog(string changeLog)
        {
            if (string.IsNullOrWhiteSpace(changeLog))
            {
                return TemplateFile.Initial;
            }

            return changeLog;
        }

        public static Task CreateEmptyAsync(string changeLogFileName)
        {
            return File.WriteAllTextAsync(path: changeLogFileName, contents: TemplateFile.Initial, encoding: Encoding.UTF8);
        }
    }
}