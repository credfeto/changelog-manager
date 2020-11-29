using System;

namespace Credfeto.ChangeLog.Exceptions
{
    public sealed class InvalidChangeLogException : Exception
    {
        public InvalidChangeLogException()
            : this("Invalid Changelog file")
        {
        }

        public InvalidChangeLogException(string message)
            : base(message)
        {
        }

        public InvalidChangeLogException(string message, Exception innerException)
            : base(message: message, innerException: innerException)
        {
        }
    }
}