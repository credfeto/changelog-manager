using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.ChangeLog.Exceptions;
using Credfeto.ChangeLog.Helpers;
using LibGit2Sharp;

namespace Credfeto.ChangeLog;

public static class ChangeLogChecker
{
    public static async Task<bool> ChangeLogModifiedInReleaseSectionAsync(string changeLogFileName, string originBranchName, CancellationToken cancellationToken)
    {
        changeLogFileName = GetFullChangeLogFilePath(changeLogFileName);
        int? position = await ChangeLogReader.FindFirstReleaseVersionPositionAsync(changeLogFileName: changeLogFileName, cancellationToken: cancellationToken);

        if (position is null)
        {
            return false;
        }

        string changelogDir = GetChangeLogDirectory(changeLogFileName);
        Console.WriteLine($"Changelog Folder: {changelogDir}");

        using (Repository repo = GitRepository.OpenRepository(changelogDir))
        {
            string sha = HeadSha(repo);

            Branch? originBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == originBranchName);

            if (originBranch is null)
            {
                throw new BranchMissingException($"Could not find branch {originBranchName}");
            }

            if (originBranch.Tip.Sha == sha)
            {
                // same branch/commit
                return false;
            }

            string changeLogInRepoPath = FindChangeLogPositionInRepo(repo: repo, changeLogFileName: changeLogFileName);
            Console.WriteLine($"Relative to Repo Root: {changeLogInRepoPath}");

            int firstReleaseVersionIndex = position.Value;

            Patch changes = repo.Diff.Compare<Patch>(oldTree: BranchTree(originBranch), newTree: HeadTree(repo), compareOptions: CompareSettings.BuildCompareOptions);

            PatchEntryChanges? change = changes.FirstOrDefault(candidate => candidate.Path == changeLogInRepoPath);

            if (change is not null)
            {
                return CheckForChangesAfterFirstRelease(change: change, firstReleaseVersionIndex: firstReleaseVersionIndex);
            }

            Console.WriteLine("Could not find change in diff");
        }

        return true;
    }

    private static Tree BranchTree(Branch branch)
    {
        return branch.Tip.Tree;
    }

    private static Tree HeadTree(Repository repo)
    {
        return BranchTree(repo.Head);
    }

    private static string BranchSha(Branch branch)
    {
        return branch.Tip.Sha;
    }

    private static string HeadSha(Repository repo)
    {
        return BranchSha(repo.Head);
    }

    private static bool CheckForChangesAfterFirstRelease(PatchEntryChanges change, int firstReleaseVersionIndex)
    {
        Console.WriteLine("Change Details");
        string patchDetails = ExtractPatchDetails(change.Patch);
        Console.WriteLine(patchDetails);

        MatchCollection matches = CommonRegex.GitHunkPosition.Matches(patchDetails);

        foreach (Match? match in matches.OfType<Match?>())
        {
            if (match is null)
            {
                continue;
            }

            int changeStart = Convert.ToInt32(value: match.Groups["CurrentFileStart"].Value, provider: CultureInfo.InvariantCulture);

            if (!int.TryParse(s: match.Groups["CurrentFileChangeLength"].Value, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, out int changeLength))
            {
                changeLength = 1;
            }

            int changeEnd = changeStart + changeLength;

            if (changeEnd >= firstReleaseVersionIndex)
            {
                return false;
            }
        }

        return true;
    }

    private static string ExtractPatchDetails(string patch)
    {
        Console.WriteLine(patch);
        List<string> lines = patch.Split('\n')
                                  .ToList();

        RemoveLastLineIfBlank(lines);

        int lastHunk = lines.FindLastIndex(x => CommonRegex.GitHunkPosition.IsMatch(x));

        if (lastHunk != -1)
        {
            (List<string> before, List<string> after) = CompareHunk(lines: lines, lastHunk: lastHunk);

            if (before.SequenceEqual(second: after, comparer: StringComparer.Ordinal))
            {
                lines.RemoveRange(index: lastHunk, lines.Count - lastHunk);
            }
        }

        return string.Join(separator: Environment.NewLine, values: lines);
    }

    private static (List<string> before, List<string> after) CompareHunk(List<string> lines, int lastHunk)
    {
        List<string> before = [];
        List<string> after = [];

        foreach (string line in lines.Skip(lastHunk + 1))
        {
            switch (line[0])
            {
                case '+':
                    after.Add(line[1..]);

                    break;

                case '-':
                    before.Add(line[1..]);

                    break;

                case '\\':
                    if (StringComparer.Ordinal.Equals(x: line, y: @"\ No newline at end of file"))
                    {
                        break;
                    }

                    return CouldNotProcessDiffLine(line);

                default: return CouldNotProcessDiffLine(line);
            }
        }

        return (before, after);
    }

    [DoesNotReturn]
    private static (List<string> before, List<string> after) CouldNotProcessDiffLine(string line)
    {
        throw new DiffException($"Could not process diff line: {line}");
    }

    private static void RemoveLastLineIfBlank(List<string> lines)
    {
        int lastLine = lines.Count - 1;

        if (lastLine < 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(lines[lastLine]))
        {
            lines.RemoveAt(lastLine);
        }
    }

    private static string GetFullChangeLogFilePath(string changeLogFileName)
    {
        FileInfo changeLog = new(changeLogFileName);

        if (!changeLog.Exists)
        {
            throw new InvalidChangeLogException($"Could not find {changeLogFileName}");
        }

        return changeLog.FullName;
    }

    private static string GetChangeLogDirectory(string changeLogFileName)
    {
        string? path = Path.GetDirectoryName(changeLogFileName);

        return path ?? Directory.GetCurrentDirectory();
    }

    private static string FindChangeLogPositionInRepo(Repository repo, string changeLogFileName)
    {
        return changeLogFileName.Substring(repo.Info.WorkingDirectory.Length)
                                .Replace(oldChar: '\\', newChar: '/');
    }
}