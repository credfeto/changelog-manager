using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Credfeto.ChangeLog.Exceptions;
using Credfeto.ChangeLog.Helpers;

namespace Credfeto.ChangeLog
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
                                .SplitToLines()
                                .ToList();

            string entryText = CreateEntryText(message);
            int index = FindInsertPosition(changeLog: text, type: type, entryText: entryText);

            if (index != -1)
            {
                text.Insert(index: index, item: entryText);
            }

            return string.Join(separator: Environment.NewLine, values: text)
                         .Trim();
        }

        private static string CreateEntryText(string message)
        {
            return "- " + message;
        }

        private static int FindInsertPosition(List<string> changeLog, string type, string entryText)
        {
            bool foundUnreleased = false;

            string search = "### " + type;

            for (int index = 0; index < changeLog.Count; index++)
            {
                string? line = changeLog[index];

                if (!foundUnreleased)
                {
                    if (line == Constants.UnreleasedHeader)
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
                            if (StringComparer.InvariantCultureIgnoreCase.Equals(x: entryText, changeLog[next]))
                            {
                                // Found matching text
                                return -1;
                            }

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

            throw new InvalidChangeLogException("Could not find [" + Constants.Unreleased + "] section of file");
        }

        public static async Task CreateReleaseAsync(string changeLogFileName, string version)
        {
            string originalChangeLog = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);

            string newChangeLog = CreateRelease(changeLog: originalChangeLog, version: version);

            await File.WriteAllTextAsync(path: changeLogFileName, contents: newChangeLog, encoding: Encoding.UTF8);
        }

        public static string CreateRelease(string changeLog, string version)
        {
            List<string> text = EnsureChangelog(changeLog)
                                .SplitToLines()
                                .ToList();

            Dictionary<string, int> releases = FindReleasePositions(text);

            if (!releases.TryGetValue(key: Constants.Unreleased, out int unreleasedIndex))
            {
                throw new EmptyChangeLogException("Could not find unreleased section");
            }

            int releaseInsertPos = FindInsertPosition(releaseVersionToFind: version, releases: releases, endOfFilePosition: text.Count);

            MoveUnreleasedToRelease(version: version, unreleasedIndex: unreleasedIndex, releaseInsertPos: releaseInsertPos, text: text);

            return string.Join(separator: Environment.NewLine, values: text)
                         .Trim();
        }

        private static void MoveUnreleasedToRelease(string version, int unreleasedIndex, int releaseInsertPos, List<string> text)
        {
            string previousLine = string.Empty;

            List<string> newRelease = new();

            List<int> removeIndexes = new();

            bool inComment = false;

            for (int i = unreleasedIndex + 1; i < releaseInsertPos; i++)
            {
                if (text[i]
                    .Contains(value: "<!--", comparisonType: StringComparison.Ordinal) && !text[i]
                    .Contains(value: "-->", comparisonType: StringComparison.Ordinal))
                {
                    if (string.IsNullOrWhiteSpace(text[i - 1]))
                    {
                        // if line before was blank then don't delete it
                        removeIndexes.Remove(i - 1);
                    }

                    inComment = true;

                    continue;
                }

                if (inComment)
                {
                    if (text[i]
                        .Contains(value: "-->", comparisonType: StringComparison.Ordinal))
                    {
                        inComment = false;
                    }

                    continue;
                }

                if (string.IsNullOrEmpty(text[i]))
                {
                    removeIndexes.Add(i);

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
                    newRelease.Add(previousLine);
                }

                removeIndexes.Add(i);
                newRelease.Add(text[i]);
                previousLine = text[i];
            }

            if (!newRelease.Any())
            {
                throw new EmptyChangeLogException();
            }

            newRelease.Insert(index: 0, "## [" + version + "] - TBD");
            newRelease.Add(string.Empty);

            text.InsertRange(index: releaseInsertPos, collection: newRelease);

            foreach (int item in removeIndexes.OrderByDescending(x => x))
            {
                text.RemoveAt(item);
            }
        }

        private static int FindInsertPosition(string releaseVersionToFind, IReadOnlyDictionary<string, int> releases, int endOfFilePosition)
        {
            string? latestRelease = releases.Keys.Where(x => x != Constants.Unreleased)
                                            .OrderByDescending(x => new Version(x))
                                            .FirstOrDefault();

            int releaseInsertPos;

            if (latestRelease != null)
            {
                Console.WriteLine($"Latest release: {latestRelease}");

                Version numericalVersion = new(releaseVersionToFind);
                Version latestNumeric = new(latestRelease);

                if (latestNumeric == numericalVersion)
                {
                    throw new ReleaseAlreadyExistsException($"Release {releaseVersionToFind} already exists");
                }

                if (latestNumeric > numericalVersion)
                {
                    throw new ReleaseTooOldException($"Release {latestRelease} already exists and is newer than {releaseVersionToFind}");
                }

                releaseInsertPos = releases[latestRelease];
            }
            else
            {
                releaseInsertPos = endOfFilePosition;
            }

            return releaseInsertPos;
        }

        private static Dictionary<string, int> FindReleasePositions(IReadOnlyList<string> text)
        {
            Dictionary<string, int> releases = text.Select((line, index) => new {line, index})
                                                   .Where(i => IsRelease(i.line))
                                                   .ToDictionary(keySelector: i => ExtractRelease(i.line), elementSelector: i => i.index);

            if (!releases.Any())
            {
                throw new EmptyChangeLogException("Could not find unreleased section");
            }

            return releases;
        }

        private static string ExtractRelease(string line)
        {
            if (line == Constants.UnreleasedHeader)
            {
                return Constants.Unreleased;
            }

            Match match = CommonRegex.VersionHeaderMatch.Match(line);

            return match.Groups["version"]
                        .Value;
        }

        private static bool IsRelease(string line)
        {
            return line == Constants.UnreleasedHeader || CommonRegex.VersionHeaderMatch.IsMatch(line);
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