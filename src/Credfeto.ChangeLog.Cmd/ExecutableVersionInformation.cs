using System;
using System.Diagnostics;

namespace Credfeto.ChangeLog.Cmd
{
    internal static class ExecutableVersionInformation
    {
        public static string ProgramVersion()
        {
            return CommonVersion(typeof(ExecutableVersionInformation));
        }

        private static string CommonVersion(Type type)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);

            return fileVersionInfo.ProductVersion;
        }
    }
}