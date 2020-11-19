using System;

namespace Credfeto.ChangeLog.Management
{
    public sealed class BranchMissingException : Exception
    {
        public BranchMissingException()
            : this("Could not find branch")
        {
        }

        public BranchMissingException(string message)
            : base(message)
        {
        }

        public BranchMissingException(string message, Exception innerException)
            : base(message: message, innerException: innerException)
        {
        }
    }
}