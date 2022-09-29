using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Credfeto.ChangeLog.Exceptions;
using Credfeto.ChangeLog.Helpers;

namespace Credfeto.ChangeLog;

public static class ChangeLogUpdater
{
    public static async Task AddEntryAsync(string changeLogFileName, string type, string message)
    {
        string textBlock = await ReadChangeLogAsync(changeLogFileName);

        string content = AddEntry(changeLog: textBlock, type: type, message: message);

        await File.WriteAllTextAsync(path: changeLogFileName, contents: content, encoding: Encoding.UTF8);
    }

    public static async Task RemoveEntryAsync(string changeLogFileName, string type, string message)
    {
        string textBlock = await ReadChangeLogAsync(changeLogFileName);

        string content = RemoveEntry(changeLog: textBlock, type: type, message: message);

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

    public static string RemoveEntry(string changeLog, string type, string message)
    {
        List<string> text = EnsureChangelog(changeLog)
                            .SplitToLines()
                            .ToList();

        string entryText = CreateEntryText(message);
        int index = FindRemovePosition(changeLog: text, type: type, entryText: entryText);

        while (index != -1)

        {
            text.RemoveAt(index: index);

            // check for another item to remove
            index = FindRemovePosition(changeLog: text, type: type, entryText: entryText);
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
        return FindMatchPosition(changeLog: changeLog,
                                 type: type,
                                 isMatch: s => StringComparer.OrdinalIgnoreCase.Equals(x: s, y: entryText),
                                 exactMatchAction: _ => -1,
                                 emptySectionAction: line => line,
                                 findSection: true);
    }

    private static int FindRemovePosition(List<string> changeLog, string type, string entryText)
    {
        return FindMatchPosition(changeLog: changeLog,
                                 type: type,
                                 isMatch: s => s.StartsWith(value: entryText, comparisonType: StringComparison.Ordinal),
                                 exactMatchAction: line => line,
                                 emptySectionAction: _ => -1,
                                 findSection: false);
    }

    private static int FindMatchPosition(List<string> changeLog, string type, Func<string, bool> isMatch, Func<int, int> exactMatchAction, Func<int, int> emptySectionAction, bool findSection)
    {
        bool foundUnreleased = false;

        string search = BuildSubHeaderSection(type);

        for (int index = 0; index < changeLog.Count; index++)
        {
            string line = changeLog[index];

            if (!foundUnreleased)
            {
                if (line == Constants.UnreleasedHeader)
                {
                    foundUnreleased = true;
                }
            }
            else
            {
                if (IsRelease(line))
                {
                    throw new InvalidChangeLogException($"Could not find {type} heading");
                }

                if (!StringComparer.Ordinal.Equals(x: line, y: search))
                {
                    continue;
                }

                int next = index + 1;

                while (next < changeLog.Count)
                {
                    if (isMatch(changeLog[next]))
                    {
                        // Found matching text
                        return exactMatchAction(next);
                    }

                    if (IsNextItem(changeLog[next]))
                    {
                        if (findSection)
                        {
                            return FindPreviousNonBlankEntry(changeLog: changeLog, earliest: index, latest: next);
                        }

                        return -1;
                    }

                    ++next;
                }

                return emptySectionAction(index + 1);
            }
        }

        throw new InvalidChangeLogException("Could not find [" + Constants.Unreleased + "] section of file");
    }

    private static string BuildSubHeaderSection(string type)
    {
        return "### " + type;
    }

    public static async Task CreateReleaseAsync(string changeLogFileName, string version, bool pending)
    {
        string originalChangeLog = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);

        string newChangeLog = CreateRelease(changeLog: originalChangeLog, version: version, pending: pending);

        await File.WriteAllTextAsync(path: changeLogFileName, contents: newChangeLog, encoding: Encoding.UTF8);
    }

    public static string CreateRelease(string changeLog, string version, bool pending)
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

        MoveUnreleasedToRelease(version: version, pending: pending, unreleasedIndex: unreleasedIndex, releaseInsertPos: releaseInsertPos, text: text);

        return string.Join(separator: Environment.NewLine, values: text)
                     .Trim();
    }

    private static void MoveUnreleasedToRelease(string version, int unreleasedIndex, int releaseInsertPos, List<string> text, bool pending)
    {
        List<string> newRelease = GenerateNewReleaseContents(unreleasedIndex: unreleasedIndex, releaseInsertPos: releaseInsertPos, text: text, out List<int> removeIndexes);

        string releaseVersionHeader = CreateReleaseVersionHeader(version: version, pending: pending);

        PrependReleaseVersionHeader(newRelease: newRelease, releaseVersionHeader: releaseVersionHeader);

        text.InsertRange(index: releaseInsertPos, collection: newRelease);

        RemoveItems(text: text, removeIndexes: removeIndexes);
    }

    private static List<string> GenerateNewReleaseContents(int unreleasedIndex, int releaseInsertPos, List<string> text, out List<int> removeIndexes)
    {
        string previousLine = string.Empty;

        List<string> newRelease = new();

        removeIndexes = new();

        bool inComment = false;

        for (int i = unreleasedIndex + 1; i < releaseInsertPos; i++)
        {
            if (SkipComments(text: text, i: i, removeIndexes: removeIndexes, inComment: ref inComment) || SkipEmptyLine(text: text, i: i, removeIndexes: removeIndexes) ||
                SkipEmptyHeadingSections(text: text, i: i, previousLine: ref previousLine) || SkipHeadingLine(text: text, i: i, previousLine: ref previousLine))
            {
                continue;
            }

            previousLine = AddLineToRelease(text: text, previousLine: previousLine, newRelease: newRelease, removeIndexes: removeIndexes, i: i);
        }

        if (newRelease.Count == 0)
        {
            throw new EmptyChangeLogException();
        }

        return newRelease;
    }

    private static void PrependReleaseVersionHeader(List<string> newRelease, string releaseVersionHeader)
    {
        newRelease.Insert(index: 0, item: releaseVersionHeader);
        newRelease.Add(string.Empty);
    }

    private static void RemoveItems(List<string> text, List<int> removeIndexes)
    {
        foreach (int item in removeIndexes.OrderByDescending(x => x))
        {
            text.RemoveAt(item);
        }
    }

    private static string CreateReleaseVersionHeader(string version, bool pending)
    {
        string releaseDate = CreateReleaseDate(pending);
        string releaseVersionHeader = "## [" + version + "] - " + releaseDate;

        return releaseVersionHeader;
    }

    private static string CreateReleaseDate(bool pending)
    {
        return pending
            ? "TBD"
            : CurrentDate();
    }

    private static string AddLineToRelease(List<string> text, string previousLine, List<string> newRelease, List<int> removeIndexes, int i)
    {
        if (IsSubHeading(previousLine))
        {
            newRelease.Add(previousLine);
        }

        removeIndexes.Add(i);
        newRelease.Add(text[i]);
        previousLine = text[i];

        return previousLine;
    }

    private static bool SkipHeadingLine(List<string> text, int i, ref string previousLine)
    {
        if (IsSubHeading(text[i]))
        {
            previousLine = text[i];

            return true;
        }

        return false;
    }

    private static bool SkipEmptyHeadingSections(List<string> text, int i, ref string previousLine)
    {
        if (IsSubHeading(text[i]) && IsSubHeading(previousLine))
        {
            previousLine = text[i];

            return true;
        }

        return false;
    }

    private static bool SkipEmptyLine(List<string> text, int i, List<int> removeIndexes)
    {
        if (string.IsNullOrEmpty(text[i]))
        {
            removeIndexes.Add(i);

            return true;
        }

        return false;
    }

    private static bool SkipComments(List<string> text, int i, List<int> removeIndexes, ref bool inComment)
    {
        if (ContainsHtmlCommentStart(text[i]) && !ContainsHtmlCommentEnd(text[i]))
        {
            if (string.IsNullOrWhiteSpace(text[i - 1]))
            {
                // if line before was blank then don't delete it
                removeIndexes.Remove(i - 1);
            }

            inComment = true;

            return true;
        }

        if (inComment)
        {
            if (ContainsHtmlCommentEnd(text[i]))
            {
                inComment = false;
            }

            return true;
        }

        return false;
    }

    private static bool ContainsHtmlCommentStart(string line)
    {
        return line.Contains(value: "<!--", comparisonType: StringComparison.Ordinal);
    }

    private static bool ContainsHtmlCommentEnd(string line)
    {
        return line.Contains(value: "-->", comparisonType: StringComparison.Ordinal);
    }

    private static bool IsSubHeading(string line)
    {
        return line.StartsWith(value: "### ", comparisonType: StringComparison.Ordinal);
    }

    [SuppressMessage(category: "FunFair.CodeAnalysis", checkId: "FFS0001", Justification = "Should always use the local time.")]
    private static string CurrentDate()
    {
        return DateTime.Now.ToString(format: "yyyy-MM-dd", provider: CultureInfo.InvariantCulture);
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
        Dictionary<string, int> releases = text.Select((line, index) => new { line, index })
                                               .Where(i => IsRelease(i.line))
                                               .ToDictionary(keySelector: i => ExtractRelease(i.line), elementSelector: i => i.index, comparer: StringComparer.Ordinal);

        if (releases.Count == 0)
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

        Match match = CommonRegex.VersionHeader.Match(line);

        return match.Groups["version"]
                    .Value;
    }

    private static bool IsRelease(string line)
    {
        return line == Constants.UnreleasedHeader || CommonRegex.VersionHeader.IsMatch(line);
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