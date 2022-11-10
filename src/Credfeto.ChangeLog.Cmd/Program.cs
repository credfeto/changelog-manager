using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Credfeto.ChangeLog.Cmd.Exceptions;

namespace Credfeto.ChangeLog.Cmd;

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
            await ExtractChangeLogTextForVersionAsync(options);

            return;
        }

        if (options.Add != null && options.Message != null)
        {
            await AddEntryToUnreleasedChangelogAsync(options);

            return;
        }

        if (options.Remove != null && options.Message != null)
        {
            await RemoveEntryFromUnreleasedChangelogAsync(options);

            return;
        }

        if (options.CheckInsert != null)
        {
            await CheckInsertPositionAsync(options);

            return;
        }

        if (options.CreateRelease != null)
        {
            await CreateNewReleaseAsync(options);

            return;
        }

        if (options.DisplayUnreleased)
        {
            await OutputUnreleasedContentAsync(options);

            return;
        }

        throw new InvalidOptionsException();
    }

    private static async Task OutputUnreleasedContentAsync(Options options)
    {
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");

        Console.WriteLine();
        Console.WriteLine("Unreleased Content:");
        string text = await ChangeLogReader.ExtractReleaseNodesFromFileAsync(changeLogFileName: changeLog, version: "0.0.0.0-unreleased");
        Console.WriteLine(text);
    }

    private static Task CreateNewReleaseAsync(Options options)
    {
        string releaseVersion = options.CreateRelease!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Release Version: {releaseVersion}");

        return ChangeLogUpdater.CreateReleaseAsync(changeLogFileName: changeLog, version: releaseVersion, pending: options.Pending);
    }

    private static async Task CheckInsertPositionAsync(Options options)
    {
        string originBranchName = options.CheckInsert!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Branch: {originBranchName}");
        bool valid = await ChangeLogChecker.ChangeLogModifiedInReleaseSectionAsync(changeLogFileName: changeLog, originBranchName: originBranchName);

        if (valid)
        {
            Console.WriteLine("Changelog is valid");

            return;
        }

        throw new ChangeLogInvalidFailedException("Changelog modified in released section");
    }

    private static Task AddEntryToUnreleasedChangelogAsync(Options options)
    {
        string changeType = options.Add!;
        string message = options.Message!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Change Type: {changeType}");
        Console.WriteLine($"Message: {message}");

        return ChangeLogUpdater.AddEntryAsync(changeLogFileName: changeLog, type: changeType, message: message);
    }

    private static Task RemoveEntryFromUnreleasedChangelogAsync(Options options)
    {
        string changeType = options.Remove!;
        string message = options.Message!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Change Type: {changeType}");
        Console.WriteLine($"Message: {message}");

        return ChangeLogUpdater.RemoveEntryAsync(changeLogFileName: changeLog, type: changeType, message: message);
    }

    private static async Task ExtractChangeLogTextForVersionAsync(Options options)
    {
        string outputFileName = options.Extract!;
        string version = options.Version!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Version {version}");

        string text = await ChangeLogReader.ExtractReleaseNodesFromFileAsync(changeLogFileName: changeLog, version: version);

        await File.WriteAllTextAsync(path: outputFileName, contents: text, encoding: Encoding.UTF8);
    }

    private static void NotParsed(IEnumerable<Error> errors)
    {
        Console.WriteLine("Errors:");

        foreach (Error error in errors)
        {
            Console.WriteLine($" * {error.Tag.GetName()}");
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

            return parser.Tag == ParserResultType.Parsed
                ? SUCCESS
                : ERROR;
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