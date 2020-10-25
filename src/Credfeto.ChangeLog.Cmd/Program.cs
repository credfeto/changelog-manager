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

                    return await ExtractAsync(changeLogFileName: changeLog, version: version, extractFileName: extractFileName);
                }

                return ERROR;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");

                return ERROR;
            }
        }

        private static async Task<int> ExtractAsync(string changeLogFileName, string version, string extractFileName)
        {
            string? text = await ChangeLogUpdater.ReadAsync(changeLogFileName: changeLogFileName, version: version);

            await File.WriteAllTextAsync(path: extractFileName, contents: text, encoding: Encoding.UTF8);

            return SUCCESS;
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
    }
}