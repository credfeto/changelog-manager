using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Credfeto.ChangeLog.Cmd;

[SuppressMessage(category: "ReSharper", checkId: "ClassNeverInstantiated.Global", Justification = "Created using reflection")]
public sealed class Options
{
    [Option(shortName: 'f', longName: "changelog", Required = false, HelpText = "The changelog filename to use")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? ChangeLog { get; init; }

    [Option(shortName: 'v', longName: "version", Group = "Commands", Required = false, HelpText = "The version to extract")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? Version { get; init; }

    [Option(shortName: 'x', longName: "extract", Group = "Commands", Required = false, HelpText = "The filename to write the extracted")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? Extract { get; init; }

    [Option(shortName: 'r', longName: "remove", Group = "Commands", Required = false, HelpText = "The entry type to remove")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? Remove { get; init; }

    [Option(shortName: 'a', longName: "add", Group = "Commands", Required = false, HelpText = "The entry type to add")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? Add { get; init; }

    [Option(shortName: 'm', longName: "message", Required = false, HelpText = "The message to add")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? Message { get; init; }

    [Option(shortName: 't', longName: "check-insert", Group = "Commands", Required = false, HelpText = "The branch to check the changelog again")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? CheckInsert { get; init; }

    [Option(shortName: 'c', longName: "create-release", Group = "Commands", Required = false, HelpText = "The release version to create")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public string? CreateRelease { get; init; }

    [Option(shortName: 'p', longName: "Pending", Required = false, HelpText = "If the release is pending")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public bool Pending { get; init; }

    [Option(shortName: 'u', longName: "un-released", Group = "Commands", Required = false, HelpText = "Prints the unreleased section to the console.")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Created using reflection")]
    public bool DisplayUnreleased { get; init; }
}