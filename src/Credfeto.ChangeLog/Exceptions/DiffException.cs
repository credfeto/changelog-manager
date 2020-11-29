using System;

namespace Credfeto.ChangeLog.Exceptions
{
    public sealed class DiffException : Exception
    {
        public DiffException()
            : this("Could not process diff")
        {
        }

        public DiffException(string message)
            : base(message)
        {
        }

        public DiffException(string message, Exception innerException)
            : base(message: message, innerException: innerException)
        {
        }
    }
}