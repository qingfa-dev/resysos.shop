namespace BuildingBlock.Domain.Unit.Test.Functional.Metadata;

using BuildingBlock.Functional.Metadata;
using Xunit;

public sealed class MetadataGuardTests
{
    #region KeyRequired Tests

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void KeyRequired_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        Action act = () => MetadataGuard.KeyRequired(key!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.key.required*");
    }

    [Fact]
    public void KeyRequired_WithValidKey_DoesNotThrow()
    {
        MetadataGuard.KeyRequired("valid-key");
    }

    #endregion

    #region KeyMaxLength Tests

    [Theory]
    [InlineData(256, "exceeding max length")]
    [InlineData(300, "well exceeding max length")]
    public void KeyMaxLength_WithKeyExceedingMaxLength_ThrowsArgumentException(int length, string _)
    {
        var key = new string('k', length);
        Action act = () => MetadataGuard.KeyMaxLength(key);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.key.max_length*");
    }

    [Theory]
    [InlineData(255, "at max length")]
    [InlineData(200, "under max length")]
    [InlineData(1, "minimum length")]
    public void KeyMaxLength_WithKeyAtOrUnderMaxLength_DoesNotThrow(int length, string _)
    {
        var key = new string('k', length);
        MetadataGuard.KeyMaxLength(key);
    }

    #endregion

    #region KeyValid Tests

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void KeyValid_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        Action act = () => MetadataGuard.KeyValid(key!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(256, "exceeding max length")]
    [InlineData(300, "well exceeding max length")]
    public void KeyValid_WithKeyExceedingMaxLength_ThrowsArgumentException(int length, string _)
    {
        var key = new string('k', length);
        Action act = () => MetadataGuard.KeyValid(key);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.key.max_length*");
    }

    [Theory]
    [InlineData(255, "at max length")]
    [InlineData(200, "under max length")]
    public void KeyValid_WithValidKey_DoesNotThrow(int length, string _)
    {
        var key = new string('k', length);
        MetadataGuard.KeyValid(key);
    }

    #endregion

    #region ValueRequired Tests

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void ValueRequired_WithIsRequiredTrueAndInvalidValue_ThrowsArgumentException(string? value, string _)
    {
        Action act = () => MetadataGuard.ValueRequired(value!, isRequired: true);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.value.required*");
    }

    [Theory]
    [InlineData("valid", "non-empty")]
    [InlineData("  value  ", "with whitespace")]
    public void ValueRequired_WithIsRequiredTrueAndValidValue_DoesNotThrow(string value, string _)
    {
        MetadataGuard.ValueRequired(value, isRequired: true);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    [InlineData("valid", "valid")]
    public void ValueRequired_WithIsRequiredFalse_DoesNotThrow(string? value, string _)
    {
        MetadataGuard.ValueRequired(value!, isRequired: false);
    }

    #endregion

    #region ValueNotNull Tests

    [Fact]
    public void ValueNotNull_WithIsRequiredTrueAndNull_ThrowsArgumentNullException()
    {
        Action act = () => MetadataGuard.ValueNotNull(null!, isRequired: true);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*metadata.value.not_null*");
    }

    [Fact]
    public void ValueNotNull_WithIsRequiredTrueAndString_DoesNotThrow()
    {
        MetadataGuard.ValueNotNull("valid", isRequired: true);
    }

    [Fact]
    public void ValueNotNull_WithIsRequiredTrueAndObject_DoesNotThrow()
    {
        MetadataGuard.ValueNotNull(new object(), isRequired: true);
    }

    [Fact]
    public void ValueNotNull_WithIsRequiredFalseAndNull_DoesNotThrow()
    {
        MetadataGuard.ValueNotNull(null!, isRequired: false);
    }

    [Fact]
    public void ValueNotNull_WithIsRequiredFalseAndObject_DoesNotThrow()
    {
        MetadataGuard.ValueNotNull(new object(), isRequired: false);
    }

    #endregion

    #region CollectionNotNull Tests

    [Fact]
    public void CollectionNotNull_WithIsRequiredTrueAndNull_ThrowsArgumentNullException()
    {
        Action act = () => MetadataGuard.CollectionRequired<string>(null!, "testCollection", isRequired: true);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*metadata.testCollection.required*");
    }

    [Fact]
    public void CollectionNotNull_WithIsRequiredTrueAndNonEmpty_DoesNotThrow()
    {
        MetadataGuard.CollectionRequired(new[] { "item" }, "testCollection", isRequired: true);
    }

    [Fact]
    public void CollectionNotNull_WithIsRequiredTrueAndEmpty_DoesNotThrow()
    {
        MetadataGuard.CollectionRequired(Array.Empty<string>(), "testCollection", isRequired: true);
    }

    [Fact]
    public void CollectionNotNull_WithIsRequiredFalseAndNull_DoesNotThrow()
    {
        MetadataGuard.CollectionRequired<string>(null!, "testCollection", isRequired: false);
    }

    [Fact]
    public void CollectionNotNull_WithIsRequiredFalseAndNonEmpty_DoesNotThrow()
    {
        MetadataGuard.CollectionRequired(new[] { "item" }, "testCollection", isRequired: false);
    }

    [Fact]
    public void CollectionNotNull_WithIsRequiredFalseAndEmpty_DoesNotThrow()
    {
        MetadataGuard.CollectionRequired(Array.Empty<string>(), "testCollection", isRequired: false);
    }

    #endregion

    #region CollectionNotEmpty Tests

    [Fact]
    public void CollectionNotEmpty_WithIsRequiredTrueAndEmpty_ThrowsArgumentException()
    {
        Action act = () => MetadataGuard.CollectionNotEmpty(Array.Empty<string>(), "testCollection", isRequired: true);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.testCollection.not_empty*");
    }

    [Fact]
    public void CollectionNotEmpty_WithIsRequiredTrueAndNonEmpty_DoesNotThrow()
    {
        MetadataGuard.CollectionNotEmpty(new[] { "item" }, "testCollection", isRequired: true);
    }

    [Fact]
    public void CollectionNotEmpty_WithIsRequiredFalseAndEmpty_DoesNotThrow()
    {
        MetadataGuard.CollectionNotEmpty(Array.Empty<string>(), "testCollection", isRequired: false);
    }

    [Fact]
    public void CollectionNotEmpty_WithIsRequiredFalseAndNonEmpty_DoesNotThrow()
    {
        MetadataGuard.CollectionNotEmpty(new[] { "item" }, "testCollection", isRequired: false);
    }

    #endregion

    #region ParameterNotNull Tests

    [Fact]
    public void ParameterNotNull_WithIsRequiredTrueAndNull_ThrowsArgumentNullException()
    {
        Action act = () => MetadataGuard.ParameterNotNull(null!, "testParam", isRequired: true);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*metadata.testParam.required*");
    }

    [Theory]
    [InlineData("valid", "non-empty")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void ParameterNotNull_WithIsRequiredTrueAndNonNull_DoesNotThrow(string parameter, string _)
    {
        MetadataGuard.ParameterNotNull(parameter, "testParam", isRequired: true);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    [InlineData("valid", "valid")]
    public void ParameterNotNull_WithIsRequiredFalse_DoesNotThrow(string? parameter, string _)
    {
        MetadataGuard.ParameterNotNull(parameter!, "testParam", isRequired: false);
    }

    #endregion

    #region ErrorMessage Tests

    [Fact]
    public void ErrorMessage_ReturnsFormattedString()
    {
        var result = MetadataGuard.ErrorMessage("TEST_CODE", "Test message");
        result.Should().Be("TEST_CODE: Test message");
    }

    #endregion
}