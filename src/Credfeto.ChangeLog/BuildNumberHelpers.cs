using System;

namespace Credfeto.ChangeLog
{
    internal static class BuildNumberHelpers
    {
        public static Version? DetermineVersionForChangeLog(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return null;
            }

            if (version.Contains('-', StringComparison.Ordinal))
            {
                return null;
            }

            return new Version(version);
        }
    }
}