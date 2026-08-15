using System;

namespace pos_service.Exceptions
{
    /// <summary>
    /// Thrown when a domain business rule validation fails (e.g. role cycle detection).
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException() { }

        public BusinessRuleException(string message) : base(message) { }

        public BusinessRuleException(string message, Exception inner) : base(message, inner) { }
    }
}
