using System;

namespace Credfeto.ChangeLog.Cmd.Exceptions
{
    public sealed class ChangeLogInvalidFailedException : Exception
    {
        public ChangeLogInvalidFailedException()
        {
        }

        public ChangeLogInvalidFailedException(string? message)
            : base(message)
        {
        }

        public ChangeLogInvalidFailedException(string? message, Exception? innerException)
            : base(message: message, innerException: innerException)
        {
        }
    }
}