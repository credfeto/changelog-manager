namespace Credfeto.ChangeLog.Management
{
    internal static class BuildNumberHelpers
    {
        public static string DetermineBuildNumberForChangeLog(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return "unreleased";
            }

            if (version.Contains('-'))
            {
                return "unreleased";
            }

            return version.Substring(startIndex: 0, version.LastIndexOf('.'));
        }
    }
}