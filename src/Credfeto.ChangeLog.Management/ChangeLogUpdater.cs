using System.Threading.Tasks;

namespace Credfeto.ChangeLog.Management
{
    public sealed class ChangeLogUpdater
    {
        public static Task<string> ReadAsync(string changeLogFileName, string version)
        {
            return Task.FromResult(string.Empty);
        }

        public static Task AddEntryAsync(string changeLogFileName, string type, string message)
        {
            return Task.CompletedTask;
        }
    }
}