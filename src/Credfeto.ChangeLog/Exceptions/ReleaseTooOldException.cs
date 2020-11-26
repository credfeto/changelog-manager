using System;

namespace Credfeto.ChangeLog.Management.Exceptions
{
    public sealed class ReleaseTooOldException : Exception
    {
        public ReleaseTooOldException()
            : this("Release is older than the current release.")
        {
        }

        public ReleaseTooOldException(string message)
            : base(message)
        {
        }

        public ReleaseTooOldException(string message, Exception innerException)
            : base(message: message, innerException: innerException)
        {
        }
    }
}