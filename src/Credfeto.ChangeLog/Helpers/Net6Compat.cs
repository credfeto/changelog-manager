using System;

namespace Credfeto.ChangeLog.Helpers;

// ReSharper disable once UnusedType.Global
internal static class Net6Compat
{
    // ReSharper disable once UnusedMember.Global
    public static readonly TimeSpan Short = TimeSpan.FromMilliseconds(RegexSettings.TimeoutMilliseconds);
}