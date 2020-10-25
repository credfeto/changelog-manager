using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Credfeto.ChangeLog.Management
{
    public sealed class ChangeLogUpdater
    {
        public static async Task AddEntryAsync(string changeLogFileName, string type, string message)
        {
            string textBlock;

            if (!File.Exists(changeLogFileName))
            {
                await CreateEmptyAsync(changeLogFileName);
                textBlock = TemplateFile.Initial;
            }
            else
            {
                textBlock = await File.ReadAllTextAsync(path: changeLogFileName, encoding: Encoding.UTF8);
            }

            string content = AddEntry(changeLog: textBlock, type: type, message: message);

            //Write-Information "* Saving Changelog"
            await File.WriteAllTextAsync(path: changeLogFileName, contents: content, encoding: Encoding.UTF8);
        }

        public static string AddEntry(string changeLog, string type, string message)
        {
            string[] text = EnsureChangelog(changeLog)
                .Split(Environment.NewLine);

            StringBuilder output = new StringBuilder();
            bool foundUnreleased = false;
            bool done = false;

            foreach (var candidateLine in text)
            {
                string line = candidateLine.TrimEnd();
                output.AppendLine(line);

                if (done)
                {
                    continue;
                }

                if (!foundUnreleased)
                {
                    if (line == "## [Unreleased]")
                    {
                        foundUnreleased = true;
                    }
                }
                else
                {
                    if (line == "### " + type)
                    {
                        //Write-Information "* Changelog Insert position added"
                        output.AppendLine("- " + message);
                        done = true;
                    }
                }
            }

            if (!done)
            {
                throw new InvalidChangeLogException("Could not find [Unreleased] section of file");
            }

            string content = output.ToString()
                                   .Trim();

            return content;
        }

        private static string EnsureChangelog(string changeLog)
        {
            if (string.IsNullOrWhiteSpace(changeLog))
            {
                return TemplateFile.Initial;
            }

            return changeLog;
        }

        public static Task CreateEmptyAsync(string changeLogFileName)
        {
            return File.WriteAllTextAsync(path: changeLogFileName, contents: TemplateFile.Initial, encoding: Encoding.UTF8);
        }
    }
}