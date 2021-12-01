using System;

namespace Credfeto.ChangeLog.Cmd.Exceptions;

public sealed class MissingChangelogException : Exception
{
    public MissingChangelogException()
    {
    }

    public MissingChangelogException(string? message)
        : base(message)
    {
    }

    public MissingChangelogException(string? message, Exception? innerException)
        : base(message: message, innerException: innerException)
    {
    }
}