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
        string text = await ChangeLogReader.ExtractReleaseNotesFromFileAsync(
            changeLogFileName: changeLog,
            version: "0.0.0.0-unreleased",
            cancellationToken: cancellationToken
        );
        Console.WriteLine(text);
    }

    private static Task CreateNewReleaseAsync(Options options, in CancellationToken cancellationToken)
    {
        string releaseVersion = GetCreateRelease(options);
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Release Version: {releaseVersion}");

        return ChangeLogUpdater.CreateReleaseAsync(
            changeLogFileName: changeLog,
            version: releaseVersion,
            pending: options.Pending,
            cancellationToken: cancellationToken
        );
    }

    private static string GetCreateRelease(Options options)
    {
        return options.CreateRelease ?? throw new InvalidOptionsException(nameof(options.CreateRelease) + " is null");
    }

    private static async Task CheckInsertPositionAsync(Options options, CancellationToken cancellationToken)
    {
        string originBranchName = GetCheckInsert(options);
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Branch: {originBranchName}");
        bool valid = await ChangeLogChecker.ChangeLogModifiedInReleaseSectionAsync(
            changeLogFileName: changeLog,
            originBranchName: originBranchName,
            cancellationToken: cancellationToken
        );

        if (valid)
        {
            Console.WriteLine("Changelog is valid");

            return;
        }

        throw new ChangeLogInvalidFailedException("Changelog modified in released section");
    }

    private static string GetCheckInsert(Options options)
    {
        return options.CheckInsert ?? throw new InvalidOptionsException(nameof(options.CheckInsert) + " is null");
    }

    private static Task AddEntryToUnreleasedChangelogAsync(Options options, in CancellationToken cancellationToken)
    {
        string changeType = GetAdd(options);
        string message = GetMessage(options);
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Change Type: {changeType}");
        Console.WriteLine($"Message: {message}");

        return ChangeLogUpdater.AddEntryAsync(
            changeLogFileName: changeLog,
            type: changeType,
            message: message,
            cancellationToken: cancellationToken
        );
    }

    private static string GetAdd(Options options)
    {
        return options.Add ?? throw new InvalidOptionsException(nameof(options.Add) + " is null");
    }

    private static Task RemoveEntryFromUnreleasedChangelogAsync(Options options, in CancellationToken cancellationToken)
    {
        string changeType = GetChangeType(options);
        string message = GetMessage(options);
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Change Type: {changeType}");
        Console.WriteLine($"Message: {message}");

        return ChangeLogUpdater.RemoveEntryAsync(
            changeLogFileName: changeLog,
            type: changeType,
            message: message,
            cancellationToken: cancellationToken
        );
    }

    private static string GetChangeType(Options options)
    {
        return options.Remove ?? throw new InvalidOptionsException(nameof(options.Remove) + " is null");
    }

    private static string GetMessage(Options options)
    {
        return options.Message ?? throw new InvalidOptionsException(nameof(options.Message) + " is null");
    }

    private static async Task ExtractChangeLogTextForVersionAsync(Options options, CancellationToken cancellationToken)
    {
        string outputFileName = GetExtract(options);
        string version = GetVersion(options);
        string changeLog = FindChangeLog(options);
        Console.WriteLine($"Using Changelog {changeLog}");
        Console.WriteLine($"Version {version}");

        string text = await ChangeLogReader.ExtractReleaseNotesFromFileAsync(
            changeLogFileName: changeLog,
            version: version,
            cancellationToken: cancellationToken
        );

        await File.WriteAllTextAsync(
            path: outputFileName,
            contents: text,
            encoding: Encoding.UTF8,
            cancellationToken: cancellationToken
        );
    }

    private static string GetVersion(Options options)
    {
        return options.Version ?? throw new InvalidOptionsException(nameof(options.Version) + " is null");
    }

    private static string GetExtract(Options options)
    {
        return options.Extract ?? throw new InvalidOptionsException(nameof(options.Extract) + " is null");
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
        Console.WriteLine($"{VersionInformation.Product} {VersionInformation.Version}");

        try
        {
            ParserResult<Options> parser = await ParseOptionsAsync(args);

            return parser.Tag == ParserResultType.Parsed ? SUCCESS : ERROR;
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

    private static Task<ParserResult<Options>> ParseOptionsAsync(IEnumerable<string> args)
    {
        return Parser.Default.ParseArguments<Options>(args).WithNotParsed(NotParsed).WithParsedAsync(ParsedOkAsync);
    }
}
