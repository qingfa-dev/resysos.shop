using BuildingBlock.Functional.Metadata;

namespace BuildingBlock.Functional.Errors;

/// <summary>
/// Error aggregation extensions.
/// </summary>
public readonly partial struct Error
{
    /// <summary>
    /// Creates an aggregate error from multiple errors.
    /// </summary>
    /// <param name="errors">The errors to aggregate.</param>
    /// <returns>A single error representing multiple violations.</returns>
    public static Error Aggregate(IEnumerable<Error> errors)
    {
        var errorList = errors?.ToList() ?? new List<Error>();
        
        if (errorList.Count == 0)
            return Error.Create(code: "error.aggregate.empty", message: "Aggregate error with no errors", type: ErrorType.Validation);
        
        if (errorList.Count == 1)
            return errorList[0];

        var codes = string.Join(", ", errorList.Select(e => e.Code));
        var messages = string.Join("; ", errorList.Select(e => e.Message));
        
        var aggregateError = Error.Create(
            code: "error.multiple_violations",
            message: $"Multiple validation errors: {messages}",
            type: ErrorType.Validation)
            .WithMetadata("error_count", errorList.Count)
            .WithMetadata("error_codes", codes);

        // Add individual errors as metadata for detailed inspection
        for (int i = 0; i < errorList.Count; i++)
        {
            aggregateError = aggregateError.WithMetadata($"error_{i}_code", errorList[i].Code)
                                           .WithMetadata($"error_{i}_message", errorList[i].Message)
                                           .WithMetadata($"error_{i}_type", errorList[i].Type.ToString());
        }

        return aggregateError;
    }

    /// <summary>
    /// Creates an aggregate error from multiple errors (params overload).
    /// </summary>
    public static Error Aggregate(params Error[] errors)
        => Aggregate((IEnumerable<Error>)errors);
}