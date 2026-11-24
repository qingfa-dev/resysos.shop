namespace BuildingBlock.Functional.Unit.Test.Errors;
public sealed class ErrorConstantTests
{
    private static readonly (ErrorType Type, string Code, string Message)[] DefaultsByType =
    [
        (ErrorConstant.Default.Unexpected.Type, ErrorConstant.Default.Unexpected.Code, ErrorConstant.Default.Unexpected.Message),
        (ErrorConstant.Default.Validation.Type, ErrorConstant.Default.Validation.Code, ErrorConstant.Default.Validation.Message),
        (ErrorConstant.Default.NotFound.Type, ErrorConstant.Default.NotFound.Code, ErrorConstant.Default.NotFound.Message),
        (ErrorConstant.Default.Conflict.Type, ErrorConstant.Default.Conflict.Code, ErrorConstant.Default.Conflict.Message),
        (ErrorConstant.Default.Unauthorized.Type, ErrorConstant.Default.Unauthorized.Code, ErrorConstant.Default.Unauthorized.Message),
        (ErrorConstant.Default.Forbidden.Type, ErrorConstant.Default.Forbidden.Code, ErrorConstant.Default.Forbidden.Message),
        (ErrorConstant.Default.BusinessRule.Type, ErrorConstant.Default.BusinessRule.Code, ErrorConstant.Default.BusinessRule.Message),
        (ErrorConstant.Default.Dependency.Type, ErrorConstant.Default.Dependency.Code, ErrorConstant.Default.Dependency.Message),
        (ErrorConstant.Default.Timeout.Type, ErrorConstant.Default.Timeout.Code, ErrorConstant.Default.Timeout.Message),
        (ErrorConstant.Default.RateLimited.Type, ErrorConstant.Default.RateLimited.Code, ErrorConstant.Default.RateLimited.Message),
        (ErrorConstant.Default.Cancelled.Type, ErrorConstant.Default.Cancelled.Code, ErrorConstant.Default.Cancelled.Message),
        (ErrorConstant.Default.NotSupported.Type, ErrorConstant.Default.NotSupported.Code, ErrorConstant.Default.NotSupported.Message),
        (ErrorConstant.Default.InvalidState.Type, ErrorConstant.Default.InvalidState.Code, ErrorConstant.Default.InvalidState.Message),
    ];

    private static readonly string[] MetadataKeys =
    [
        ErrorConstant.Metadata.TraceId,
        ErrorConstant.Metadata.Timestamp,
        ErrorConstant.Metadata.Resource,
        ErrorConstant.Metadata.Field,
        ErrorConstant.Metadata.Attempt,
        ErrorConstant.Metadata.ExceptionType,
        ErrorConstant.Metadata.ExceptionMessage,
        ErrorConstant.Metadata.InnerException,
    ];

    #region Code Constraints

    [Theory]
    [InlineData("error.code.required", "The code is required.")]
    public void CodeRequired_IsExpected(string code, string message)
    {
        ErrorConstant.Constraints.Code.Required.Code.Should().Be(code);
        ErrorConstant.Constraints.Code.Required.Message.Should().Be(message);
    }

    [Theory]
    [InlineData(100, "error.code.max_length", "The code exceeds the maximum length of 100 characters.")]
    public void CodeMaxLength_IsExpected(int value, string code, string message)
    {
        ErrorConstant.Constraints.Code.MaxLength.Value.Should().Be(value);
        ErrorConstant.Constraints.Code.MaxLength.Code.Should().Be(code);
        ErrorConstant.Constraints.Code.MaxLength.Message.Should().Be(message);
    }

    [Fact]
    public void CodePattern_HasExpectedDefinition()
    {
        ErrorConstant.Constraints.Code.Pattern.Value.Should().Be(
            @"^[a-z][a-z0-9]*(?:_[a-z0-9]+){0,5}(?:\.[a-z][a-z0-9]*(?:_[a-z0-9]+){0,5}){0,5}$");
        ErrorConstant.Constraints.Code.Pattern.Code.Should().Be("error.code.pattern");
        ErrorConstant.Constraints.Code.Pattern.Message.Should().Be(
            "Error code must be 1-6 dot-separated segments in lowercase snake_case (each up to 6 words, e.g. 'metadata.key.not_found').");
    }

    [Theory]
    [InlineData("valid.code")]
    [InlineData("validation.failed")]
    [InlineData("resource.not_found")]
    [InlineData("a.b.c.d.e.f")]
    [InlineData("invalid_code")]
    [InlineData("invalid.code1")]
    public void CodePattern_AcceptsValidCodes(string code)
    {
        ErrorConstant.Constraints.Code.Pattern.Regex.IsMatch(code).Should().BeTrue();
    }

    [Theory]
    [InlineData("Invalid.Code")]
    [InlineData("INVALID.CODE")]
    [InlineData("1invalid.code")]
    [InlineData("invalid..code")]
    [InlineData("")]
    public void CodePattern_RejectsInvalidCodes(string code)
    {
        ErrorConstant.Constraints.Code.Pattern.Regex.IsMatch(code).Should().BeFalse();
    }

    #endregion

    #region Message Constraints

    [Theory]
    [InlineData("error.message.required", "The message is required.")]
    public void MessageRequired_IsExpected(string code, string message)
    {
        ErrorConstant.Constraints.Message.Required.Code.Should().Be(code);
        ErrorConstant.Constraints.Message.Required.Message.Should().Be(message);
    }

    [Theory]
    [InlineData(500, "error.message.max_length", "The message exceeds the maximum length of 500 characters.")]
    public void MessageMaxLength_IsExpected(int value, string code, string message)
    {
        ErrorConstant.Constraints.Message.MaxLength.Value.Should().Be(value);
        ErrorConstant.Constraints.Message.MaxLength.Code.Should().Be(code);
        ErrorConstant.Constraints.Message.MaxLength.Message.Should().Be(message);
    }

    #endregion

    #region Type Constraints

    [Theory]
    [InlineData("error.type.required", "The type is required.")]
    public void TypeRequired_IsExpected(string code, string message)
    {
        ErrorConstant.Constraints.Type.Required.Code.Should().Be(code);
        ErrorConstant.Constraints.Type.Required.Message.Should().Be(message);
    }

    [Theory]
    [InlineData("error.type.out_of_range", "The type is out of the allowed range.")]
    public void TypeOutOfRange_IsExpected(string code, string message)
    {
        ErrorConstant.Constraints.Type.OutOfRange.Code.Should().Be(code);
        ErrorConstant.Constraints.Type.OutOfRange.Message.Should().Be(message);
    }

    #endregion

    #region Default

    [Fact]
    public void Default_IsExpected()
    {
        ErrorConstant.Default.Type.Should().Be(ErrorType.Unexpected);
        ErrorConstant.Default.Code.Should().Be("error.default");
        ErrorConstant.Default.Message.Should().Be("An unexpected error occurred.");
        ErrorConstant.Default.NoneType.Should().Be(ErrorType.None);
    }

    [Theory]
    [InlineData(0, ErrorType.Unexpected, "error.unexpected", "An unexpected error occurred.")]
    [InlineData(1, ErrorType.Validation, "validation.failed", "The supplied data failed validation.")]
    [InlineData(2, ErrorType.NotFound, "resource.not_found", "The requested resource could not be found.")]
    [InlineData(3, ErrorType.Conflict, "resource.conflict", "The requested operation conflicts with the current resource state.")]
    [InlineData(4, ErrorType.Unauthorized, "access.unauthorized", "Authentication is required to perform this operation.")]
    [InlineData(5, ErrorType.Forbidden, "access.forbidden", "You do not have permission to perform this operation.")]
    [InlineData(6, ErrorType.BusinessRule, "business.rule_violation", "The requested operation violates a business rule.")]
    [InlineData(7, ErrorType.Dependency, "dependency.failed", "A required external dependency failed to complete the operation.")]
    [InlineData(8, ErrorType.Timeout, "system.timeout", "The operation exceeded the allowed execution time.")]
    [InlineData(9, ErrorType.RateLimited, "system.rate_limited", "The allowed request limit has been exceeded.")]
    [InlineData(10, ErrorType.Cancelled, "system.cancelled", "The operation was cancelled.")]
    [InlineData(11, ErrorType.NotSupported, "error.not_supported", "The requested operation is not supported.")]
    [InlineData(12, ErrorType.InvalidState, "error.invalid_state", "The operation cannot be performed in the current state.")]
    public void DefaultByType_IsExpected(int index, ErrorType type, string code, string message)
    {
        var actual = DefaultsByType[index];

        actual.Type.Should().Be(type);
        actual.Code.Should().Be(code);
        actual.Message.Should().Be(message);
    }

    #endregion

    #region Metadata

    [Theory]
    [InlineData(0, "TraceId")]
    [InlineData(1, "Timestamp")]
    [InlineData(2, "Resource")]
    [InlineData(3, "Field")]
    [InlineData(4, "Attempt")]
    [InlineData(5, "ExceptionType")]
    [InlineData(6, "ExceptionMessage")]
    [InlineData(7, "InnerException")]
    public void MetadataKey_IsExpected(int index, string expected)
    {
        MetadataKeys[index].Should().Be(expected);
    }

    #endregion
}
