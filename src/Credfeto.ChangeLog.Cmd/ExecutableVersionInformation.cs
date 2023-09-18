namespace Credfeto.ChangeLog.Cmd;

internal static class ExecutableVersionInformation
{
    public static string ProgramVersion()
    {
        return ThisAssembly.Info.FileVersion;
    }
}