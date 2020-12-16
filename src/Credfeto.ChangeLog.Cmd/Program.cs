using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
                IConfigurationRoot configuration = LoadConfiguration(args);

                string? changeLog = configuration.GetValue<string>(key: @"changelog");

                if (string.IsNullOrEmpty(changeLog))
                {
                    if (!ChangeLogDetector.TryFindChangeLog(out changeLog))
                    {
                        Console.WriteLine("ERROR: changelog not specified or found");

                        return ERROR;
                    }
                }

                Console.WriteLine($"Using Changelog {changeLog}");

                string extractFileName = configuration.GetValue<string>("extract");

                if (!string.IsNullOrEmpty(extractFileName))
                {
                    string version = configuration.GetValue<string>("version");

                    Console.WriteLine($"Version: {version}");

                    string text = await ChangeLogReader.ExtractReleaseNodesFromFileAsync(changeLogFileName: changeLog, version: version);

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

                    Console.WriteLine($"Change Type: {addType}");
                    Console.WriteLine($"Message: {message}");

                    await ChangeLogUpdater.AddEntryAsync(changeLogFileName: changeLog, type: addType, message: message);

                    return SUCCESS;
                }

                string? branchName = configuration.GetValue<string>("check-insert");

                if (!string.IsNullOrWhiteSpace(branchName))
                {
                    Console.WriteLine($"Branch: {branchName}");
                    bool valid = await ChangeLogChecker.ChangeLogModifiedInReleaseSectionAsync(changeLogFileName: changeLog, originBranchName: branchName);

                    if (valid)
                    {
                        Console.WriteLine("Changelog is valid");

                        return SUCCESS;
                    }

                    await Console.Error.WriteLineAsync("ERROR: Changelog modified in released section");

                    return ERROR;
                }

                string? releaseVersion = configuration.GetValue<string>("create-release");

                if (!string.IsNullOrWhiteSpace(releaseVersion))
                {
                    Console.WriteLine($"Release Version: {releaseVersion}");

                    // TODO: Add in Release Date setting
                    // TODO: Add in command to Set date of an already released release
                    await ChangeLogUpdater.CreateReleaseAsync(changeLogFileName: changeLog, version: releaseVersion);

                    return SUCCESS;
                }

                Console.WriteLine("ERROR: No known action specified");

                return ERROR;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");

                if (exception.StackTrace != null)
                {
                    Console.WriteLine(exception.StackTrace);
                }

                return ERROR;
            }
        }

        private static IConfigurationRoot LoadConfiguration(string[] args)
        {
            return new ConfigurationBuilder().AddCommandLine(args: args,
                                                             new Dictionary<string, string>
                                                             {
                                                                 {@"-help", @"help"},
                                                                 {@"--help", @"help"},
                                                                 {@"-changelog", @"changelog"},
                                                                 {@"--changelog", @"changelog"},
                                                                 {@"-c", @"changelog"},
                                                                 {@"-version", @"version"},
                                                                 {@"--version", @"version"},
                                                                 {@"-v", @"version"},
                                                                 {@"-extract", @"extract"},
                                                                 {@"--extract", @"extract"},
                                                                 {@"-add", @"add"},
                                                                 {@"--add", @"add"},
                                                                 {@"-a", @"add"},
                                                                 {@"-message", @"message"},
                                                                 {@"--message", @"message"},
                                                                 {@"-m", @"message"},
                                                                 {@"-check-insert", @"check-insert"},
                                                                 {@"--check-insert", @"check-insert"},
                                                                 {@"-create-release", "create-release"},
                                                                 {@"--create-release", "create-release"}
                                                             })
                                             .Build();
        }
    }
}