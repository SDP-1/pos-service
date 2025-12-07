using System;

namespace pos_service.Exceptions
{
    /// <summary>
    /// Thrown when a user lacks the required permission for an action.
    /// Derives from UnauthorizedAccessException for compatibility with existing handlers.
    /// </summary>
    public class PermissionDeniedException : UnauthorizedAccessException
    {
        public PermissionDeniedException() { }

        public PermissionDeniedException(string message) : base(message) { }

        public PermissionDeniedException(string message, Exception inner) : base(message, inner) { }
    }
}
