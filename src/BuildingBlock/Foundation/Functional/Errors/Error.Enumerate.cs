namespace BuildingBlock.Functional.Errors;

/// <summary>
/// Defines the classification of an error.
/// </summary>
public enum ErrorType : int
{
    /// <summary>
    /// No error type specified (default).
    /// </summary>
    None = -1,

    /// <summary>
    /// An unexpected or unclassified error.
    /// </summary>
    Unexpected = 0,

    /// <summary>
    /// Invalid input or failed data validation.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// A requested resource could not be found.
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// A resource or operation conflicts with the current state.
    /// </summary>
    Conflict = 3,

    /// <summary>
    /// Authentication is required or has failed.
    /// </summary>
    Unauthorized = 4,

    /// <summary>
    /// The caller does not have sufficient permissions.
    /// </summary>
    Forbidden = 5,

    /// <summary>
    /// A domain business rule or invariant was violated.
    /// </summary>
    BusinessRule = 6,

    /// <summary>
    /// An external service or dependency failed.
    /// </summary>
    Dependency = 7,

    /// <summary>
    /// An operation exceeded its permitted execution time.
    /// </summary>
    Timeout = 8,

    /// <summary>
    /// The caller exceeded an allowed request or operation limit.
    /// </summary>
    RateLimited = 9,

    /// <summary>
    /// An operation was cancelled.
    /// </summary>
    Cancelled = 10,

    /// <summary>
    /// The requested operation is not supported.
    /// </summary>
    NotSupported = 11,

    /// <summary>
    /// An operation failed because of an invalid application state.
    /// </summary>
    InvalidState = 12
}