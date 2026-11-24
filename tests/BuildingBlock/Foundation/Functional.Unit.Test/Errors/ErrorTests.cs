namespace BuildingBlock.Domain.Unit.Test.Functional.Errors;

using BuildingBlock.Functional.Errors;
using Xunit;

public sealed class ErrorTests
{
    private static readonly Func<Error>[] Factories =
    [
        () => Error.Unexpected(),
        () => Error.Validation(),
        () => Error.NotFound(),
        () => Error.Conflict(),
        () => Error.Unauthorized(),
        () => Error.Forbidden(),
        () => Error.BusinessRule(),
        () => Error.Dependency(),
        () => Error.Timeout(),
        () => Error.RateLimited(),
        () => Error.Cancelled(),
        () => Error.NotSupported(),
        () => Error.InvalidState(),
    ];

    #region Construction

    [Theory]
    [InlineData("test.code", "Test message", ErrorType.Validation)]
    [InlineData("validation.failed", "The supplied data failed validation.", ErrorType.Validation)]
    [InlineData("resource.not_found", "missing", ErrorType.NotFound)]
    public void Create_WithValidArguments_SetsProperties(
        string code,
        string message,
        ErrorType type)
    {
        var error = Error.Create(code, message, type);

        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(type);
        error.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithDefaults_UsesDefaultConstants()
    {
        var error = Error.Create();

        error.Code.Should().Be(ErrorConstant.Default.Code);
        error.Message.Should().Be(ErrorConstant.Default.Message);
        error.Type.Should().Be(ErrorConstant.Default.Type);
    }

    [Fact]
    public void Create_WithNullCode_ThrowsArgumentNullException()
    {
        Action act = () => Error.Create(null!, "message", ErrorType.Validation);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankCode_ThrowsArgumentException(string code)
    {
        Action act = () => Error.Create(code, "message", ErrorType.Validation);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.code.required*");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    public void Create_WithCodeExceedingMaxLength_ThrowsArgumentException(int length)
    {
        Action act = () => Error.Create(new string('c', length), "message", ErrorType.Validation);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.code.max_length*");
    }

    [Theory]
    [InlineData("Invalid.Code")]
    [InlineData("INVALID.CODE")]
    [InlineData("1invalid.code")]
    [InlineData("invalid..code")]
    public void Create_WithInvalidCodePattern_ThrowsArgumentException(string code)
    {
        Action act = () => Error.Create(code, "message", ErrorType.Validation);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.code.pattern*");
    }

    [Fact]
    public void Create_WithNullMessage_ThrowsArgumentNullException()
    {
        Action act = () => Error.Create("test.code", null!, ErrorType.Validation);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankMessage_ThrowsArgumentException(string message)
    {
        Action act = () => Error.Create("test.code", message, ErrorType.Validation);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.message.required*");
    }

    [Theory]
    [InlineData(501)]
    [InlineData(1000)]
    public void Create_WithMessageExceedingMaxLength_ThrowsArgumentException(int length)
    {
        Action act = () => Error.Create("test.code", new string('m', length), ErrorType.Validation);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.message.max_length*");
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(999)]
    public void Create_WithInvalidType_ThrowsArgumentOutOfRangeException(int type)
    {
        Action act = () => Error.Create("test.code", "message", (ErrorType)type);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Typed Factories

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
    public void TypedFactory_CreatesErrorWithDefaults(
        int index,
        ErrorType type,
        string code,
        string message)
    {
        var error = Factories[index]();

        error.Type.Should().Be(type);
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
    }

    #endregion

    #region With Methods

    [Fact]
    public void WithCode_ReturnsUpdatedErrorPreservingOtherValues()
    {
        var original = Error.Create("original.code", "message", ErrorType.Validation);

        var updated = original.WithCode("new.code");

        updated.Should().NotBe(original);
        updated.Code.Should().Be("new.code");
        updated.Message.Should().Be("message");
        updated.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("Invalid.Code")]
    [InlineData("")]
    public void WithCode_WithInvalidCode_ThrowsArgumentException(string code)
    {
        var original = Error.Create("test.code", "message", ErrorType.Validation);

        Action act = () => original.WithCode(code);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithCode_WithNullCode_ThrowsArgumentNullException()
    {
        var original = Error.Create("test.code", "message", ErrorType.Validation);

        Action act = () => original.WithCode(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithMessage_ReturnsUpdatedErrorPreservingOtherValues()
    {
        var original = Error.Create("test.code", "original message", ErrorType.Validation);

        var updated = original.WithMessage("new message");

        updated.Should().NotBe(original);
        updated.Message.Should().Be("new message");
        updated.Code.Should().Be("test.code");
        updated.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void WithMessage_WithBlankMessage_ThrowsArgumentException(string message)
    {
        var original = Error.Create("test.code", "message", ErrorType.Validation);

        Action act = () => original.WithMessage(message);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithType_ReturnsUpdatedErrorPreservingOtherValues()
    {
        var original = Error.Create("test.code", "message", ErrorType.Validation);

        var updated = original.WithType(ErrorType.NotFound);

        updated.Should().NotBe(original);
        updated.Type.Should().Be(ErrorType.NotFound);
        updated.Code.Should().Be("test.code");
        updated.Message.Should().Be("message");
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(999)]
    public void WithType_WithInvalidType_ThrowsArgumentOutOfRangeException(int type)
    {
        var original = Error.Create("test.code", "message", ErrorType.Validation);

        Action act = () => original.WithType((ErrorType)type);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Metadata Composition

    [Fact]
    public void WithMetadata_AddsValueWithoutMutatingOriginal()
    {
        var original = Error.Create("test.code", "message", ErrorType.Validation);

        var updated = original.WithMetadata("key", "value");

        updated.Metadata.GetMetadata("key").Should().Be("value");
        original.Metadata.HasMetadata("key").Should().BeFalse();
    }

    [Fact]
    public void WithMetadata_PreservesExistingMetadata()
    {
        var original = Error.Create().WithMetadata("first", 1);

        var updated = original.WithMetadata("second", 2);

        updated.Metadata.GetMetadata("first").Should().Be(1);
        updated.Metadata.GetMetadata("second").Should().Be(2);
        original.Metadata.HasMetadata("second").Should().BeFalse();
    }

    [Fact]
    public void WithMetadata_OnDefaultError_CreatesMetadata()
    {
        var updated = default(Error).WithMetadata("key", "value");

        updated.Metadata.GetMetadata("key").Should().Be("value");
    }

    #endregion

    #region Well-known Metadata

    [Fact]
    public void WithTraceId_SetsTraceId()
    {
        var traceId = Guid.NewGuid();

        var error = Error.Create().WithTraceId(traceId);

        error.GetTraceId().Should().Be(traceId);
        error.Metadata.GetMetadata(ErrorConstant.Metadata.TraceId).Should().Be(traceId);
    }

    [Fact]
    public void GetTraceId_WithStringValue_ParsesGuid()
    {
        var traceId = Guid.NewGuid();

        var error = Error.Create().WithMetadata(
            ErrorConstant.Metadata.TraceId,
            traceId.ToString());

        error.GetTraceId().Should().Be(traceId);
    }

    [Fact]
    public void GetTraceId_WithUnparsableValue_ReturnsNull()
    {
        var error = Error.Create().WithMetadata(
            ErrorConstant.Metadata.TraceId,
            "not-a-guid");

        error.GetTraceId().Should().BeNull();
    }

    [Fact]
    public void GetTraceId_WithoutTraceId_ReturnsNull()
    {
        Error.Create().GetTraceId().Should().BeNull();
    }

    [Fact]
    public void WithTimestamp_SetsTimestamp()
    {
        var timestamp = DateTimeOffset.UtcNow;

        var error = Error.Create().WithTimestamp(timestamp);

        error.GetTimestamp().Should().Be(timestamp);
    }

    [Fact]
    public void GetTimestamp_WithoutTimestamp_ReturnsNull()
    {
        Error.Create().GetTimestamp().Should().BeNull();
    }

    [Fact]
    public void WithResource_SetsResource()
    {
        var error = Error.Create().WithResource("resource-123");

        error.GetResource().Should().Be("resource-123");
    }

    [Fact]
    public void GetResource_WithoutResource_ReturnsNull()
    {
        Error.Create().GetResource().Should().BeNull();
    }

    [Fact]
    public void WithField_SetsField()
    {
        var error = Error.Create().WithField("email");

        error.GetField().Should().Be("email");
    }

    [Fact]
    public void GetField_WithoutField_ReturnsNull()
    {
        Error.Create().GetField().Should().BeNull();
    }

    [Fact]
    public void GetField_WithEmptyField_ReturnsNull()
    {
        var error = Error.Create().WithMetadata(ErrorConstant.Metadata.Field, "");

        error.GetField().Should().BeNull();
    }

    [Fact]
    public void WithAttempt_SetsAttempt()
    {
        var error = Error.Create().WithAttempt(3);

        error.GetAttempt().Should().Be(3);
    }

    [Fact]
    public void GetAttempt_WithoutAttempt_ReturnsNull()
    {
        Error.Create().GetAttempt().Should().BeNull();
    }

    [Fact]
    public void WithExceptionType_SetsExceptionType()
    {
        var error = Error.Create().WithExceptionType(typeof(ArgumentException));

        error.GetExceptionType().Should().Be(typeof(ArgumentException));
    }

    [Fact]
    public void GetExceptionType_WithoutExceptionType_ReturnsNull()
    {
        Error.Create().GetExceptionType().Should().BeNull();
    }

    [Fact]
    public void WithExceptionMessage_SetsExceptionMessage()
    {
        var error = Error.Create().WithExceptionMessage("Something went wrong");

        error.GetExceptionMessage().Should().Be("Something went wrong");
    }

    [Fact]
    public void GetExceptionMessage_WithoutExceptionMessage_ReturnsNull()
    {
        Error.Create().GetExceptionMessage().Should().BeNull();
    }

    [Fact]
    public void GetExceptionMessage_WithEmptyMessage_ReturnsNull()
    {
        var error = Error.Create().WithMetadata(ErrorConstant.Metadata.ExceptionMessage, "");

        error.GetExceptionMessage().Should().BeNull();
    }

    [Fact]
    public void WithException_SetsExceptionTypeAndMessage()
    {
        var exception = new InvalidOperationException("Operation failed");

        var error = Error.Create().WithException(exception);

        error.GetExceptionType().Should().Be(typeof(InvalidOperationException));
        error.GetExceptionMessage().Should().Be("Operation failed");
    }

    [Fact]
    public void WithException_WithInnerException_SetsInnerException()
    {
        var inner = new ArgumentException("Inner error");
        var exception = new InvalidOperationException("Outer error", inner);

        var error = Error.Create().WithException(exception);

        error.GetInnerException().Should().Be(inner);
    }

    [Fact]
    public void WithException_WithNullException_ThrowsArgumentNullException()
    {
        Action act = () => Error.Create().WithException(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetInnerException_WithoutInnerException_ReturnsNull()
    {
        Error.Create().GetInnerException().Should().BeNull();
    }

    [Fact]
    public void DefaultError_WellKnownGetters_ReturnNull()
    {
        var error = default(Error);

        error.GetTraceId().Should().BeNull();
        error.GetTimestamp().Should().BeNull();
        error.GetResource().Should().BeNull();
        error.GetField().Should().BeNull();
        error.GetAttempt().Should().BeNull();
        error.GetExceptionType().Should().BeNull();
        error.GetExceptionMessage().Should().BeNull();
        error.GetInnerException().Should().BeNull();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var error1 = Error.Create("test.code", "message", ErrorType.Validation);
        var error2 = Error.Create("test.code", "message", ErrorType.Validation);

        error1.Equals(error2).Should().BeTrue();
    }

    [Fact]
    public void Equals_IgnoresMetadata()
    {
        var error1 = Error.Create("test.code", "message", ErrorType.Validation)
            .WithTraceId(Guid.NewGuid());
        var error2 = Error.Create("test.code", "message", ErrorType.Validation);

        error1.Should().Be(error2);
        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Theory]
    [InlineData("code2", "message", ErrorType.Validation)]
    [InlineData("code", "message2", ErrorType.Validation)]
    [InlineData("code", "message", ErrorType.NotFound)]
    public void Equals_WithAnyDifferentValue_ReturnsFalse(
        string code,
        string message,
        ErrorType type)
    {
        var error1 = Error.Create("code", "message", ErrorType.Validation);
        var error2 = Error.Create(code, message, type);

        error1.Equals(error2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var error = Error.Create("test.code", "message", ErrorType.Validation);

        error.Equals(error).Should().BeTrue();
    }

    [Fact]
    public void EqualsObject_WithNull_ReturnsFalse()
    {
        var error = Error.Create("test.code", "message", ErrorType.Validation);

        error.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_WithNonError_ReturnsFalse()
    {
        var error = Error.Create("test.code", "message", ErrorType.Validation);

        error.Equals("not an error").Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_WithBoxedError_ReturnsTrue()
    {
        var error1 = Error.Create("test.code", "message", ErrorType.Validation);
        object error2 = Error.Create("test.code", "message", ErrorType.Validation);

        error1.Equals(error2).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EqualityOperators_AgreeWithEquals(bool sameValues)
    {
        var error1 = Error.Create("test.code", "message", ErrorType.Validation);
        var error2 = sameValues
            ? Error.Create("test.code", "message", ErrorType.Validation)
            : Error.Create("other.code", "message", ErrorType.Validation);

        (error1 == error2).Should().Be(sameValues);
        (error1 != error2).Should().Be(!sameValues);
        error1.Equals(error2).Should().Be(sameValues);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var error1 = Error.Create("test.code", "message", ErrorType.Validation);
        var error2 = Error.Create("test.code", "message", ErrorType.Validation);

        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHash()
    {
        var error1 = Error.Create("test.code", "message", ErrorType.Validation);
        var error2 = Error.Create("other.code", "message", ErrorType.Validation);

        error1.GetHashCode().Should().NotBe(error2.GetHashCode());
    }

    [Fact]
    public void IEquatable_Equals_Works()
    {
        var error1 = Error.Create("test.code", "message", ErrorType.Validation);
        IEquatable<Error> equatable = error1;
        var error2 = Error.Create("test.code", "message", ErrorType.Validation);

        equatable.Equals(error2).Should().BeTrue();
    }

    #endregion

    #region Conversions

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var error = Error.Create("test.code", "Test message", ErrorType.Validation);

        error.ToString().Should().Be("Error: Code=test.code, Message=Test message, Type=Validation");
    }

    [Fact]
    public void ToException_ReturnsExceptionWithToStringMessage()
    {
        var error = Error.Create("test.code", "Test message", ErrorType.Validation);

        var exception = error.ToException();

        exception.Should().BeOfType<Exception>();
        exception.Message.Should().Be(error.ToString());
    }

    #endregion
}
