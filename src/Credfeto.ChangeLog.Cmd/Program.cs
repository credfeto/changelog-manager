using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Credfeto.ChangeLog.Management;
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

                    await ChangeLogUpdater.AddEntryAsync(changeLogFileName: changeLog, type: addType, message: message);

                    return SUCCESS;
                }

                string? branchName = configuration.GetValue<string>("check-insert");

                if (!string.IsNullOrWhiteSpace(branchName))
                {
                    bool valid = await ChangeLogChecker.ChangeLogModifiedInReleaseSectionAsync(changeLogFileName: changeLog, originBranchName: branchName);

                    if (valid)
                    {
                        Console.WriteLine("Changelog is valid");

                        return SUCCESS;
                    }

                    await Console.Error.WriteLineAsync("ERROR: Changelog modified in released section");

                    return ERROR;
                }

                Console.WriteLine("ERROR: No known action specified");

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
                                                                 {@"-message", @"message"},
                                                                 {@"-check-insert", @"check-insert"}
                                                             })
                                             .Build();
        }
    }
}