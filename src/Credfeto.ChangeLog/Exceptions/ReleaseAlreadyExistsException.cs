using System;

namespace Credfeto.ChangeLog.Exceptions
{
    public sealed class ReleaseAlreadyExistsException : Exception
    {
        public ReleaseAlreadyExistsException()
            : this("Release already exists.")
        {
        }

        public ReleaseAlreadyExistsException(string message)
            : base(message)
        {
        }

        public ReleaseAlreadyExistsException(string message, Exception innerException)
            : base(message: message, innerException: innerException)
        {
        }
    }
}