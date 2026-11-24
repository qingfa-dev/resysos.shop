using System.Text.RegularExpressions;

namespace BuildingBlock.Functional.Errors;

public static partial class ErrorConstant
{
    #region Contraints
    public static partial class Constraints
    {
        public static partial class Code
        {
            public static class Required
            {
                public const string Code = "error.code.required";
                public static readonly string Message = "The code is required.";
            }
            public static class MaxLength
            {
                public const int Value = 100;
                public const string Code = "error.code.max_length";
                public static readonly string Message = $"The code exceeds the maximum length of {Value} characters.";
            }
            public static partial class Pattern
            {
                public const string Value =
                @"^[a-z][a-z0-9]*(?:_[a-z0-9]+){0,5}(?:\.[a-z][a-z0-9]*(?:_[a-z0-9]+){0,5}){0,5}$";

                public static readonly Regex Regex = ErrorCodeRegex();

                [GeneratedRegex(Value, RegexOptions.Compiled | RegexOptions.CultureInvariant)]
                private static partial Regex ErrorCodeRegex();

                public const string Code = "error.code.pattern";
                public const string Message = "Error code must be 1-6 dot-separated segments in lowercase snake_case (each up to 6 words, e.g. 'metadata.key.not_found').";
            }
        }
        public static class Message
        {
            public static class Required
            {
                public const string Code = "error.message.required";
                public const string Message = "The message is required.";
            }
            public static class MaxLength
            {
                public const int Value = 500;
                public const string Code = "error.message.max_length";
                public static readonly string Message = $"The message exceeds the maximum length of {Value} characters.";
            }
        }
        public static class Type
        {
            public static class Required
            {
                public const string Code = "error.type.required";
                public const string Message = "The type is required.";
            }
            public static class OutOfRange
            {
                public const string Code = "error.type.out_of_range";
                public const string Message = "The type is out of the allowed range.";
            }
        }
    }
    #endregion

    public static class Default
    {
        #region Default Error
        public const ErrorType Type = ErrorType.Unexpected;
        public const string Code = "error.default";
        public const string Message = "An unexpected error occurred.";
        #endregion

        #region None Error
        public const ErrorType NoneType = ErrorType.None;
        #endregion

        #region Default Error By Type

        public static class Unexpected
        {
            public const ErrorType Type = ErrorType.Unexpected;
            public const string Code = "error.unexpected";
            public const string Message = "An unexpected error occurred.";
        }

        public static class Validation
        {
            public const ErrorType Type = ErrorType.Validation;
            public const string Code = "validation.failed";
            public const string Message = "The supplied data failed validation.";
        }

        public static class NotFound
        {
            public const ErrorType Type = ErrorType.NotFound;
            public const string Code = "resource.not_found";
            public const string Message = "The requested resource could not be found.";
        }

        public static class Conflict
        {
            public const ErrorType Type = ErrorType.Conflict;
            public const string Code = "resource.conflict";
            public const string Message = "The requested operation conflicts with the current resource state.";
        }

        public static class Unauthorized
        {
            public const ErrorType Type = ErrorType.Unauthorized;
            public const string Code = "access.unauthorized";
            public const string Message = "Authentication is required to perform this operation.";
        }

        public static class Forbidden
        {
            public const ErrorType Type = ErrorType.Forbidden;
            public const string Code = "access.forbidden";
            public const string Message = "You do not have permission to perform this operation.";
        }

        public static class BusinessRule
        {
            public const ErrorType Type = ErrorType.BusinessRule;
            public const string Code = "business.rule_violation";
            public const string Message = "The requested operation violates a business rule.";
        }

        public static class Dependency
        {
            public const ErrorType Type = ErrorType.Dependency;
            public const string Code = "dependency.failed";
            public const string Message = "A required external dependency failed to complete the operation.";
        }

        public static class Timeout
        {
            public const ErrorType Type = ErrorType.Timeout;
            public const string Code = "system.timeout";
            public const string Message = "The operation exceeded the allowed execution time.";
        }

        public static class RateLimited
        {
            public const ErrorType Type = ErrorType.RateLimited;
            public const string Code = "system.rate_limited";
            public const string Message = "The allowed request limit has been exceeded.";
        }

        public static class Cancelled
        {
            public const ErrorType Type = ErrorType.Cancelled;
            public const string Code = "system.cancelled";
            public const string Message = "The operation was cancelled.";
        }

        public static class NotSupported
        {
            public const ErrorType Type = ErrorType.NotSupported;
            public const string Code = "error.not_supported";
            public const string Message = "The requested operation is not supported.";
        }

        public static class InvalidState
        {
            public const ErrorType Type = ErrorType.InvalidState;
            public const string Code = "error.invalid_state";
            public const string Message = "The operation cannot be performed in the current state.";
        }
        #endregion
    }

    #region Wellknown metadata
    /// <summary>
    /// Standard metadata keys used across errors and results.
    /// </summary>
    public static class Metadata
    {
        /// <summary>
        /// Key for the distributed trace identifier.
        /// </summary>
        public const string TraceId = "TraceId";

        /// <summary>
        /// Key for the error timestamp.
        /// </summary>
        public const string Timestamp = "Timestamp";

        /// <summary>
        /// Key for the affected resource identifier.
        /// </summary>
        public const string Resource = "Resource";

        /// <summary>
        /// Key for the field identifier (e.g., validation field).
        /// </summary>
        public const string Field = "Field";

        /// <summary>
        /// Key for the retry attempt number.
        /// </summary>
        public const string Attempt = "Attempt";

        /// <summary>
        /// Key for the type of the provider exception behind a failure.
        /// </summary>
        public const string ExceptionType = "ExceptionType";

        /// <summary>
        /// Key for the text of the provider exception behind a failure.
        /// </summary>
        public const string ExceptionMessage = "ExceptionMessage";
        /// <summary>
        /// Key for the inner exception of the provider exception behind a failure.
        /// </summary>
        public const string InnerException = "InnerException";
    }
    #endregion
}

