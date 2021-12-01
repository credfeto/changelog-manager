using System;

namespace Credfeto.ChangeLog.Helpers;

internal static class RegexTimeouts
{
    public static TimeSpan Short { get; } = TimeSpan.FromSeconds(1);
}