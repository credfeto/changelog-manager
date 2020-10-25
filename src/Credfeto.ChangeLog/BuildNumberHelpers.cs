using System;

namespace Credfeto.ChangeLog.Management
{
    internal static class BuildNumberHelpers
    {
        public static Version? DetermineVersionForChangeLog(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return null;
            }

            if (version.Contains('-'))
            {
                return null;
            }

            return new Version(version);

            //return version.Substring(startIndex: 0, version.LastIndexOf('.'));
        }
    }
}