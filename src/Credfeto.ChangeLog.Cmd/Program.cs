using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Credfeto.ChangeLog.Management;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;

namespace Credfeto.ChangeLog.Cmd
{
    internal static class Program
    {
        private const int SUCCESS = 0;
        private const int ERROR = 1;

        private static async Task<int> Main(string[] args)
        {
            Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

            try
            {
                string workDir = Environment.CurrentDirectory;

                using (Repository repo = OpenRepository(workDir))
                {
                    string? sha = repo.Head.Tip.Sha;
                    Console.WriteLine($"Head SHA: {sha}");

                    const string originBranchName = "master";

                    Branch? originBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == originBranchName);

                    if (originBranch == null)
                    {
                        // Can't find origin branch - error
                    }
                    else
                    {
                        if (originBranch.Tip.Sha == sha)
                        {
                            // same branch
                        }
                        else
                        {
                            // Blob oldblob = repo.Lookup<Blob>(originBranch.Tip.Sha);
                            // Blob newblob = repo.Lookup<Blob>(repo.Head.Tip.Sha);

                            Regex versionMatchRegex = new(pattern: @"^##\s\[(\d+)", options: RegexOptions.Compiled);

                            string[] changelog = await File.ReadAllLinesAsync(@"D:\Work\changelog-manager\CHANGELOG.md");

                            int firstReleaseVersionIndex = -1;

                            for (int lineIndex = 0; lineIndex < changelog.Length; ++lineIndex)
                            {
                                string line = changelog[lineIndex];

                                if (versionMatchRegex.IsMatch(line))
                                {
                                    firstReleaseVersionIndex = lineIndex;

                                    break;
                                }
                            }

                            if (firstReleaseVersionIndex != -1)
                            {
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine($"First release version: {firstReleaseVersionIndex}");

                                Patch changes = repo.Diff.Compare<Patch>(oldTree: originBranch.Tip.Tree,
                                                                         newTree: repo.Head.Tip.Tree,
                                                                         new CompareOptions {ContextLines = 0, InterhunkLines = 0, IncludeUnmodified = false});

                                Regex regex = new(pattern: @"^@@\s*\-(?<OriginalFileStart>\d*)(,(?<OriginalFileEnd>\d*))?\s*\+(?<CurrentFileStart>\d*)(,(?<CurrentFileChangeLength>\d*))?\s*@@",
                                                  RegexOptions.Compiled | RegexOptions.Multiline);

                                foreach (var change in changes)
                                {
                                    if (change.Path == "CHANGELOG.md")
                                    {
                                        string patchDetails = change.Patch;
                                        Console.WriteLine(patchDetails);

                                        MatchCollection matches = regex.Matches(patchDetails);

                                        foreach (Match match in matches)
                                        {
                                            int changeStart = Convert.ToInt32(match.Groups["CurrentFileStart"]
                                                                                   .Value);

                                            if (!int.TryParse(s: match.Groups["CurrentFileChangeLength"]
                                                                      .Value,
                                                              out int changeLength))
                                            {
                                                changeLength = 1;
                                            }

                                            int changeEnd = changeStart + changeLength;
                                            Console.WriteLine($"Hunk Start: {changeStart}");
                                            Console.WriteLine($"Hunk End: {changeEnd}");

                                            if (changeEnd >= firstReleaseVersionIndex)
                                            {
                                                Console.WriteLine("---- Error - Content modified after First release");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                IConfigurationRoot configuration = LoadConfiguration(args);

                string changeLog = configuration.GetValue<string>(key: @"changelog");

                if (string.IsNullOrEmpty(changeLog))
                {
                    Console.WriteLine("ERROR: changelog not specified");

                    return ERROR;
                }

                string extractFileName = configuration.GetValue<string>("extract");

                if (!string.IsNullOrEmpty(extractFileName))
                {
                    string version = configuration.GetValue<string>("version");

                    string text = await ChangeLogReader.ExtractReleasNodesFromFileAsync(changeLogFileName: changeLog, version: version);

                    await File.WriteAllTextAsync(path: extractFileName, contents: text, encoding: Encoding.UTF8);

                    return SUCCESS;
                }

                string? addType = configuration.GetValue<string>("add");

                if (!string.IsNullOrEmpty(addType))
                {
                    string message = configuration.GetValue<string>("message");

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine("ERROR: message not specified");

                        return ERROR;
                    }

                    await ChangeLogUpdater.AddEntryAsync(changeLogFileName: changeLog, type: addType, message: message);

                    return SUCCESS;
                }

                return ERROR;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");

                return ERROR;
            }
        }

        private static IConfigurationRoot LoadConfiguration(string[] args)
        {
            return new ConfigurationBuilder().AddCommandLine(args: args,
                                                             new Dictionary<string, string>
                                                             {
                                                                 {@"-changelog", @"changelog"},
                                                                 {@"-version", @"version"},
                                                                 {@"-extract", @"extract"},
                                                                 {@"-add", @"add"},
                                                                 {@"-message", @"message"}
                                                             })
                                             .Build();
        }

        private static Repository OpenRepository(string workDir)
        {
            string found = Repository.Discover(workDir);

            return new Repository(found);
        }
    }
}