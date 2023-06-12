using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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

        if (changeLog is not null)
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
        CancellationToken cancellationToken = CancellationToken.None;

        if (options.Extract is not null && options.Version is not null)
        {
            await ExtractChangeLogTextForVersionAsync(options: options, cancellationToken: cancellationToken);

            return;
        }

        if (options.Add is not null && options.Message is not null)
        {
            await AddEntryToUnreleasedChangelogAsync(options: options, cancellationToken: cancellationToken);

            return;
        }

        if (options.Remove is not null && options.Message is not null)
        {
            await RemoveEntryFromUnreleasedChangelogAsync(options: options, cancellationToken: cancellationToken);

            return;
        }

        if (options.CheckInsert is not null)
        {
            await CheckInsertPositionAsync(options: options, cancellationToken: cancellationToken);

            return;
        }

        if (options.CreateRelease is not null)
        {
            await CreateNewReleaseAsync(options: options, cancellationToken: cancellationToken);

            return;
        }

        if (options.DisplayUnreleased)
        {
            await OutputUnreleasedContentAsync(options: options, cancellationToken: cancellationToken);

            return;
        }

        throw new InvalidOptionsException();
    }

    private static async Task OutputUnreleasedContentAsync(Options options, CancellationToken cancellationToken)
    {
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");

        Console.WriteLine();
        Console.WriteLine("Unreleased Content:");
        string text = await ChangeLogReader.ExtractReleaseNodesFromFileAsync(changeLogFileName: changeLog, version: "0.0.0.0-unreleased", cancellationToken: cancellationToken);
        Console.WriteLine(text);
    }

    private static Task CreateNewReleaseAsync(Options options, in CancellationToken cancellationToken)
    {
        string releaseVersion = options.CreateRelease!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Release Version: {releaseVersion}");

        return ChangeLogUpdater.CreateReleaseAsync(changeLogFileName: changeLog, version: releaseVersion, pending: options.Pending, cancellationToken: cancellationToken);
    }

    private static async Task CheckInsertPositionAsync(Options options, CancellationToken cancellationToken)
    {
        string originBranchName = options.CheckInsert!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Branch: {originBranchName}");
        bool valid = await ChangeLogChecker.ChangeLogModifiedInReleaseSectionAsync(changeLogFileName: changeLog, originBranchName: originBranchName, cancellationToken: cancellationToken);

        if (valid)
        {
            Console.WriteLine("Changelog is valid");

            return;
        }

        throw new ChangeLogInvalidFailedException("Changelog modified in released section");
    }

    private static Task AddEntryToUnreleasedChangelogAsync(Options options, in CancellationToken cancellationToken)
    {
        string changeType = options.Add!;
        string message = options.Message!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Change Type: {changeType}");
        Console.WriteLine($"Message: {message}");

        return ChangeLogUpdater.AddEntryAsync(changeLogFileName: changeLog, type: changeType, message: message, cancellationToken: cancellationToken);
    }

    private static Task RemoveEntryFromUnreleasedChangelogAsync(Options options, in CancellationToken cancellationToken)
    {
        string changeType = options.Remove!;
        string message = options.Message!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Change Type: {changeType}");
        Console.WriteLine($"Message: {message}");

        return ChangeLogUpdater.RemoveEntryAsync(changeLogFileName: changeLog, type: changeType, message: message, cancellationToken: cancellationToken);
    }

    private static async Task ExtractChangeLogTextForVersionAsync(Options options, CancellationToken cancellationToken)
    {
        string outputFileName = options.Extract!;
        string version = options.Version!;
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Version {version}");

        string text = await ChangeLogReader.ExtractReleaseNodesFromFileAsync(changeLogFileName: changeLog, version: version, cancellationToken: cancellationToken);

        await File.WriteAllTextAsync(path: outputFileName, contents: text, encoding: Encoding.UTF8, cancellationToken: cancellationToken);
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

            if (exception.StackTrace is not null)
            {
                Console.WriteLine(exception.StackTrace);
            }

            return ERROR;
        }
    }
}