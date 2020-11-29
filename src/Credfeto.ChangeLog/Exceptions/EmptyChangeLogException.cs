using System;

namespace Credfeto.ChangeLog.Exceptions
{
    public sealed class EmptyChangeLogException : Exception
    {
        public EmptyChangeLogException()
            : this("Changelog does not contain content.")
        {
        }

        public EmptyChangeLogException(string message)
            : base(message)
        {
        }

        public EmptyChangeLogException(string message, Exception innerException)
            : base(message: message, innerException: innerException)
        {
        }
    }
}