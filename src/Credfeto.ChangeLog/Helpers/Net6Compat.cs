using System;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.ChangeLog.Helpers;

[SuppressMessage(
    category: "ReSharper",
    checkId: "UnusedType.Global",
    Justification = "TODO: Review"
)]
internal static class Net6Compat
{
    [SuppressMessage(
        category: "ReSharper",
        checkId: "UnusedMember.Global",
        Justification = "TODO: Review"
    )]
    public static readonly TimeSpan Short = TimeSpan.FromMilliseconds(
        RegexSettings.TimeoutMilliseconds
    );
}
