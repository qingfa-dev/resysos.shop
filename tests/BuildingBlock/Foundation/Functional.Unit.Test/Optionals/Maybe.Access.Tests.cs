namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using BuildingBlock.Functional.Optionals;
using Xunit;

public sealed class MaybeAccessTests
{
    #region Value Property

    [Fact]
    public void Value_WhenEmpty_ThrowsDomainException()
    {
        var empty = Maybe<int>.Empty();

        Action act = () => _ = empty.Value;

        act.Should().Throw<DomainException>()
            .Where(exception => exception.Error.Code == "maybe.value.required");
    }

    #endregion

    #region ToResult

    [Fact]
    public void ToResult_Should_Map_Presence_And_Absence()
    {
        Maybe<int> some = Maybe<int>.Some(3);
        Maybe<int> empty = Maybe<int>.Empty();

        Result<int> present = some.ToResult(Error.Validation("missing", "missing"));
        Result<int> absent = empty.ToResult(Error.Validation("missing", "missing"));

        present.Value.Should().Be(3);
        absent.IsFailure.Should().BeTrue();
        absent.Errors[0].Code.Should().Be("missing");
    }

    #endregion

    #region TryGetValue

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void TryGetValue_WhenSome_ReturnsTrue(int value)
    {
        var result = Maybe<int>.Some(value).TryGetValue(out var actual);

        result.Should().BeTrue();
        actual.Should().Be(value);
    }

    [Fact]
    public void TryGetValue_WhenEmpty_ReturnsFalseAndDefault()
    {
        var result = Maybe<int>.Empty().TryGetValue(out var actual);

        result.Should().BeFalse();
        actual.Should().Be(0);
    }

    [Fact]
    public void TryGetValue_WithReferenceType_WhenEmpty_ReturnsFalseAndNull()
    {
        var result = Maybe<string>.Empty().TryGetValue(out var actual);

        result.Should().BeFalse();
        actual.Should().BeNull();
    }

    #endregion

    #region GetValueOrDefault

    [Theory]
    [InlineData(42, -1, 42)]
    [InlineData(0, 9, 0)]
    public void GetValueOrDefault_WhenSome_ReturnsValue(int value, int fallback, int expected)
    {
        Maybe<int>.Some(value).GetValueOrDefault(fallback).Should().Be(expected);
    }

    [Fact]
    public void GetValueOrDefault_WhenEmpty_ReturnsFallback()
    {
        Maybe<int>.Empty().GetValueOrDefault(9).Should().Be(9);
    }

    #endregion

    #region Deconstruct

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void Deconstruct_WhenSome_ReturnsTrueAndValue(int value)
    {
        var (hasValue, actual) = Maybe<int>.Some(value);

        hasValue.Should().BeTrue();
        actual.Should().Be(value);
    }

    [Fact]
    public void Deconstruct_WhenEmpty_ReturnsFalseAndDefault()
    {
        var (hasValue, actual) = Maybe<int>.Empty();

        hasValue.Should().BeFalse();
        actual.Should().Be(0);
    }

    #endregion
}