using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Credfeto.ChangeLog.Exceptions;
using Credfeto.ChangeLog.Helpers;
using LibGit2Sharp;

namespace Credfeto.ChangeLog
{
    public static class ChangeLogChecker
    {
        private static readonly Regex HunkPositionRegex =
            new(pattern: @"^@@\s*\-(?<OriginalFileStart>\d*)(,(?<OriginalFileEnd>\d*))?\s*\+(?<CurrentFileStart>\d*)(,(?<CurrentFileChangeLength>\d*))?\s*@@",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        public static async Task<bool> ChangeLogModifiedInReleaseSectionAsync(string changeLogFileName, string originBranchName)
        {
            changeLogFileName = GetFullChangeLogFilePath(changeLogFileName);
            int? position = await ChangeLogReader.FindFirstReleaseVersionPositionAsync(changeLogFileName);

            if (position == null)
            {
                return false;
            }

            string changelogDir = GetChangeLogDirectory(changeLogFileName);
            Console.WriteLine($"Changelog Folder: {changelogDir}");

            using (Repository repo = GitRepository.OpenRepository(changelogDir))
            {
                string sha = repo.Head.Tip.Sha;

                Branch? originBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == originBranchName);

                if (originBranch == null)
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

                Patch changes = repo.Diff.Compare<Patch>(oldTree: originBranch.Tip.Tree,
                                                         newTree: repo.Head.Tip.Tree,
                                                         new CompareOptions { ContextLines = 0, InterhunkLines = 0, IncludeUnmodified = false });

                PatchEntryChanges? change = changes.FirstOrDefault(candidate => candidate.Path == changeLogInRepoPath);

                if (change != null)
                {
                    return CheckForChangesAfterFirstRelease(change: change, firstReleaseVersionIndex: firstReleaseVersionIndex);
                }

                Console.WriteLine("Could not find change in diff");
            }

            return true;
        }

        private static bool CheckForChangesAfterFirstRelease(PatchEntryChanges change, int firstReleaseVersionIndex)
        {
            Console.WriteLine("Change Details");
            string patchDetails = ExtractPatchDetails(change.Patch);
            Console.WriteLine(patchDetails);

            MatchCollection matches = HunkPositionRegex.Matches(patchDetails);

            foreach (Match? match in matches)
            {
                if (match == null)
                {
                    continue;
                }

                int changeStart = Convert.ToInt32(match.Groups["CurrentFileStart"]
                                                       .Value);

                if (!int.TryParse(s: match.Groups["CurrentFileChangeLength"]
                                          .Value,
                                  out int changeLength))
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

            int lastHunk = lines.FindLastIndex(x => HunkPositionRegex.IsMatch(x));

            if (lastHunk != -1)
            {
                CompareHunk(lines: lines, lastHunk: lastHunk, out List<string> before, out List<string> after);

                if (before.SequenceEqual(after))
                {
                    lines.RemoveRange(index: lastHunk, lines.Count - lastHunk);
                }
            }

            return string.Join(separator: Environment.NewLine, values: lines);
        }

        private static void CompareHunk(List<string> lines, int lastHunk, out List<string> before, out List<string> after)
        {
            before = new List<string>();
            after = new List<string>();

            foreach (string line in lines.Skip(lastHunk + 1))
            {
                switch (line[0])
                {
                    case '+':
                        after.Add(line.Substring(1));

                        break;

                    case '-':
                        before.Add(line.Substring(1));

                        break;

                    case '\\':
                        if (line == @"\ No newline at end of file")
                        {
                            break;
                        }

                        throw new DiffException($"Could not process diff line: {line}");

                    default: throw new DiffException($"Could not process diff line: {line}");
                }
            }
        }

        private static void RemoveLastLineIfBlank(List<string> lines)
        {
            int lastLine = lines.Count - 1;

            if (lastLine >= 0)
            {
                if (string.IsNullOrEmpty(lines[lastLine]))
                {
                    lines.RemoveAt(lastLine);
                }
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
                                    .Replace(oldValue: "\\", newValue: "/");
        }
    }
}