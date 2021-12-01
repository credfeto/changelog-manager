using System;

namespace Credfeto.ChangeLog.Tests;

internal static class StringExtensions
{
    public static string ToLocalEndLine(this string source)
    {
        return source.Replace(oldValue: "\r\n", newValue: Environment.NewLine, comparisonType: StringComparison.Ordinal);
    }
}