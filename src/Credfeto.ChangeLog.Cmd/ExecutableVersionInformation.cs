using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Credfeto.ChangeLog.Cmd;

internal static class ExecutableVersionInformation
{
    public static string ProgramVersion()
    {
        return CommonVersion(typeof(Program));
    }

    private static string CommonVersion(Type type)
    {
        Assembly assembly = type.Assembly;

        return GetAssemblyFileVersionFile() ?? GetAssemblyFileVersion(assembly) ?? GetAssemblyVersion(assembly);
    }

    private static string? GetAssemblyFileVersionFile()
    {
        IReadOnlyList<string> args = Environment.GetCommandLineArgs();

        if (args.Count == 0)
        {
            return null;
        }

        string location = args[0];

        if (string.IsNullOrWhiteSpace(location) || !File.Exists(location))
        {
            return null;
        }

        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(location);

        return fileVersionInfo.ProductVersion!;
    }

    private static string? GetAssemblyFileVersion(Assembly assembly)
    {
        AssemblyFileVersionAttribute? fvi = assembly.GetCustomAttributes<AssemblyFileVersionAttribute>()
                                                    .FirstOrDefault();

        return fvi?.Version;
    }

    private static string GetAssemblyVersion(Assembly assembly)
    {
        Console.Write("Finding Assembly Version");
        Version? assemblyVersion = assembly.GetName()
                                           .Version;

        if (assemblyVersion is null)
        {
            throw new VersionNotFoundException();
        }

        return assemblyVersion.ToString();
    }
}