using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Credfeto.ChangeLog.Cmd.Exceptions;

namespace Credfeto.ChangeLog.Cmd
{
    internal static class Program
    {
        private const int SUCCESS = 0;
        private const int ERROR = 1;

        private static string FindChangeLog(Options options)
        {
            string? changeLog = options.ChangeLog;

            if (changeLog != null)
            {
                return changeLog;
            }

            if (ChangeLogDetector.TryFindChangeLog(out changeLog))
            {
                return changeLog;
            }

            throw new MissingChangelogException("Could not find changelog");
        }

        private static async Task ParsedOkAsync(Options options)
        {
            if (options.Extract != null && options.Version != null)
            {
                string changeLog = FindChangeLog(options);
                Console.WriteLine($"Using Changelog {changeLog}");

                string text = await ChangeLogReader.ExtractReleaseNodesFromFileAsync(changeLogFileName: changeLog, version: options.Version);

                await File.WriteAllTextAsync(path: options.Extract, contents: text, encoding: Encoding.UTF8);

                return;
            }

            if (options.Add != null && options.Message != null)
            {
                string changeLog = FindChangeLog(options);
                Console.WriteLine($"Using Changelog {changeLog}");
                Console.WriteLine($"Change Type: {options.Add}");
                Console.WriteLine($"Message: {options.Message}");

                await ChangeLogUpdater.AddEntryAsync(changeLogFileName: changeLog, type: options.Add, message: options.Message);

                return;
            }

            if (options.CheckInsert != null)
            {
                string changeLog = FindChangeLog(options);
                Console.WriteLine($"Using Changelog {changeLog}");
                Console.WriteLine($"Branch: {options.CheckInsert}");
                bool valid = await ChangeLogChecker.ChangeLogModifiedInReleaseSectionAsync(changeLogFileName: changeLog, originBranchName: options.CheckInsert);

                if (valid)
                {
                    Console.WriteLine("Changelog is valid");

                    return;
                }

                throw new ChangeLogInvalidFailedException("Changelog modified in released section");
            }

            if (options.CreateRelease != null)
            {
                string changeLog = FindChangeLog(options);
                Console.WriteLine($"Using Changelog {changeLog}");
                Console.WriteLine($"Release Version: {options.CreateRelease}");

                // TODO: Add in Release Date setting
                // TODO: Add in command to Set date of an already released release
                await ChangeLogUpdater.CreateReleaseAsync(changeLogFileName: changeLog, version: options.CreateRelease);

                return;
            }

            throw new InvalidOptionsException();
        }

        private static void NotParsed(IEnumerable<Error> errors)
        {
            Console.WriteLine("Errors:");

            foreach (Error error in errors)
            {
                Console.WriteLine($" * {error.Tag} - {error}");
            }
        }

        private static async Task<int> Main(string[] args)
        {
            Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

            try
            {
                ParserResult<Options> parser = await Parser.Default.ParseArguments<Options>(args)
                                                           .WithNotParsed(NotParsed)
                                                           .WithParsedAsync(ParsedOkAsync);

                return parser.Tag == ParserResultType.Parsed ? SUCCESS : ERROR;
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
    }
}