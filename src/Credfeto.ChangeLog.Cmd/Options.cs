using CommandLine;

namespace Credfeto.ChangeLog.Cmd
{
    public sealed class Options
    {
        [Option(shortName: 'f', longName: "change-log", Required = false, HelpText = "The changelog filename to use")]
        public string? ChangeLog { get; init; }

        [Option(shortName: 'v', longName: "version", SetName = "Extract", Required = false, HelpText = "The version to extract")]
        public string? Version { get; init; }

        [Option(shortName: 'x', longName: "extract", SetName = "Extract", Required = false, HelpText = "The filename to write the extracted")]
        public string? Extract { get; init; }

        [Option(shortName: 'a', longName: "add", SetName = "NewEntry", Required = false, HelpText = "The entry type to add")]
        public string? Add { get; init; }

        [Option(shortName: 'm', longName: "message", SetName = "NewEntry", Required = false, HelpText = "The message to add")]
        public string? Message { get; init; }

        [Option(shortName: 't', longName: "check-insert", Required = false, HelpText = "The branch to check the changelog again")]
        public string? CheckInsert { get; init; }

        [Option(shortName: 'c', longName: "create-release", Required = false, HelpText = "The release version to create")]
        public string? CreateRelease { get; init; }
    }
}