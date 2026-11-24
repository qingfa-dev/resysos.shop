namespace BuildingBlock.Domain.Unit.Test.Functional.Errors;

using BuildingBlock.Functional.Errors;
using Xunit;

public sealed class ErrorGuardTests
{
    #region Code Tests

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void CodeRequired_WithInvalidCode_ThrowsArgumentException(string? code, string _)
    {
        Action act = () => ErrorGuard.CodeRequired(code!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.code.required*");
    }

    [Fact]
    public void CodeRequired_WithValidCode_DoesNotThrow()
    {
        ErrorGuard.CodeRequired("valid.code");
    }

    [Theory]
    [InlineData(101, "exceeding max length")]
    [InlineData(200, "well exceeding max length")]
    public void CodeMaxLength_WithCodeExceedingMaxLength_ThrowsArgumentException(int length, string _)
    {
        var code = new string('c', length);
        Action act = () => ErrorGuard.CodeMaxLength(code);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.code.max_length*");
    }

    [Theory]
    [InlineData(100, "at max length")]
    [InlineData(50, "under max length")]
    public void CodeMaxLength_WithCodeAtOrUnderMaxLength_DoesNotThrow(int length, string _)
    {
        var code = new string('c', length);
        ErrorGuard.CodeMaxLength(code);
    }

    [Theory]
    [InlineData("Invalid.Code", "camel case")]
    [InlineData("INVALID.CODE", "uppercase")]
    [InlineData("1invalid.code", "starts with number")]
    [InlineData("invalid..code", "double dot")]
    public void CodePattern_WithInvalidPattern_ThrowsArgumentException(string code, string _)
    {
        Action act = () => ErrorGuard.CodePattern(code);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.code.pattern*");
    }

    [Theory]
    [InlineData("valid.code", "simple")]
    [InlineData("validation.failed", "with dot")]
    [InlineData("resource.not_found", "snake case with dot")]
    [InlineData("a.b.c.d.e.f", "six segments")]
    [InlineData("invalid_code", "underscore")]
    [InlineData("invalid.code1", "ends with number")]
    public void CodePattern_WithValidPattern_DoesNotThrow(string code, string _)
    {
        ErrorGuard.CodePattern(code);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    [InlineData("Invalid.Code", "invalid pattern")]
    public void CodeValid_WithInvalidCode_ThrowsArgumentException(string? code, string _)
    {
        Action act = () => ErrorGuard.CodeValid(code!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CodeValid_WithValidCode_DoesNotThrow()
    {
        ErrorGuard.CodeValid("valid.code");
    }

    #endregion

    #region Message Tests

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void MessageRequired_WithInvalidMessage_ThrowsArgumentException(string? message, string _)
    {
        Action act = () => ErrorGuard.MessageRequired(message!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.message.required*");
    }

    [Fact]
    public void MessageRequired_WithValidMessage_DoesNotThrow()
    {
        ErrorGuard.MessageRequired("Valid message");
    }

    [Theory]
    [InlineData(501, "exceeding max length")]
    [InlineData(1000, "well exceeding max length")]
    public void MessageMaxLength_WithMessageExceedingMaxLength_ThrowsArgumentException(int length, string _)
    {
        var message = new string('m', length);
        Action act = () => ErrorGuard.MessageMaxLength(message);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*error.message.max_length*");
    }

    [Theory]
    [InlineData(500, "at max length")]
    [InlineData(250, "under max length")]
    public void MessageMaxLength_WithMessageAtOrUnderMaxLength_DoesNotThrow(int length, string _)
    {
        var message = new string('m', length);
        ErrorGuard.MessageMaxLength(message);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void MessageValid_WithInvalidMessage_ThrowsArgumentException(string? message, string _)
    {
        Action act = () => ErrorGuard.MessageValid(message!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MessageValid_WithMessageExceedingMaxLength_ThrowsArgumentException()
    {
        var message = new string('m', 501);
        Action act = () => ErrorGuard.MessageValid(message);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MessageValid_WithValidMessage_DoesNotThrow()
    {
        ErrorGuard.MessageValid("Valid message");
    }

    #endregion

    #region Type Tests

    [Fact]
    public void TypeRequired_WithNegativeType_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => ErrorGuard.TypeRequired(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TypeRequired_WithZero_DoesNotThrow()
    {
        ErrorGuard.TypeRequired(0); // ErrorType.Unexpected = 0
    }

    [Fact]
    public void TypeRequired_WithPositiveType_DoesNotThrow()
    {
        ErrorGuard.TypeRequired(1); // ErrorType.Validation = 1
    }

    [Fact]
    public void TypeOutOfRange_WithUndefinedType_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => ErrorGuard.TypeOutOfRange(999);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*error.type.out_of_range*");
    }

    [Fact]
    public void TypeOutOfRange_WithNegativeType_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => ErrorGuard.TypeOutOfRange(-2);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TypeOutOfRange_WithValidType_DoesNotThrow()
    {
        ErrorGuard.TypeOutOfRange(0); // Unexpected
        ErrorGuard.TypeOutOfRange(1); // Validation
        ErrorGuard.TypeOutOfRange(12); // InvalidState
    }

    [Theory]
    [InlineData(-2, "negative")]
    [InlineData(999, "undefined")]
    public void TypeValid_WithInvalidType_ThrowsArgumentOutOfRangeException(int type, string _)
    {
        Action act = () => ErrorGuard.TypeValid(type);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TypeValid_WithValidType_DoesNotThrow()
    {
        ErrorGuard.TypeValid(0); // Unexpected
        ErrorGuard.TypeValid(12); // InvalidState
    }

    #endregion

    #region Null / Whitespace Short-Circuit Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CodeMaxLength_WithNullOrWhitespace_DoesNotThrow(string? code)
    {
        ErrorGuard.CodeMaxLength(code!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CodePattern_WithNullOrWhitespace_DoesNotThrow(string? code)
    {
        ErrorGuard.CodePattern(code!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MessageMaxLength_WithNullOrWhitespace_DoesNotThrow(string? message)
    {
        ErrorGuard.MessageMaxLength(message!);
    }

    [Fact]
    public void CodeRequired_WithNull_ThrowsArgumentNullException()
    {
        Action act = () => ErrorGuard.CodeRequired(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CodeRequired_WithBlank_ThrowsArgumentException(string code)
    {
        var exception = Record.Exception(() => ErrorGuard.CodeRequired(code));

        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void MessageRequired_WithNull_ThrowsArgumentNullException()
    {
        Action act = () => ErrorGuard.MessageRequired(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MessageRequired_WithBlank_ThrowsArgumentException(string message)
    {
        var exception = Record.Exception(() => ErrorGuard.MessageRequired(message));

        exception.Should().BeOfType<ArgumentException>();
    }

    #endregion

    #region ErrorMessage Tests

    [Fact]
    public void ErrorMessage_ReturnsFormattedString()
    {
        var result = ErrorGuard.ErrorMessage("TEST_CODE", "Test message");
        result.Should().Be("TEST_CODE: Test message");
    }

    [Fact]
    public void ErrorMessage_WithEmptyMessage_ReturnsCodeOnly()
    {
        var result = ErrorGuard.ErrorMessage("TEST_CODE", "");
        result.Should().Be("TEST_CODE: ");
    }

    #endregion
}