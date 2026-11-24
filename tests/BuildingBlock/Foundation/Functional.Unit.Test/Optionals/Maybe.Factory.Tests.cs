namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;

using Xunit;

public sealed class MaybeFactoryTests
{
    #region Some

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Some_WithValue_ReturnsSome(int value)
    {
        var maybe = Maybe<int>.Some(value);

        maybe.HasValue.Should().BeTrue();
        maybe.HasNoValue.Should().BeFalse();
        maybe.Value.Should().Be(value);
    }

    [Fact]
    public void Some_WithNullReference_ThrowsArgumentNullException()
    {
        Action act = () => Maybe<string>.Some(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*maybe.value.required*");
    }

    #endregion

    #region Empty

    [Fact]
    public void Empty_ReturnsNone()
    {
        var maybe = Maybe<int>.Empty();

        maybe.HasValue.Should().BeFalse();
        maybe.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region FromNullable

    [Fact]
    public void FromNullable_WithReferenceValue_ReturnsSome()
    {
        var maybe = Maybe<string>.FromNullable("value");

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be("value");
    }

    [Fact]
    public void FromNullable_WithNullReference_ReturnsEmpty()
    {
        var maybe = Maybe<string>.FromNullable(null);

        maybe.HasValue.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void FromNullable_WithStructValue_ReturnsSome(int value)
    {
        var maybe = Maybe<int>.FromNullable(value);

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(value);
    }

    [Fact]
    public void FromNullable_WithStructValue_Zero_ReturnsSome()
    {
        // For value types, FromNullable receives the value directly (not nullable)
        // The caller is responsible for checking HasValue on Nullable<T> before calling
        var maybe = Maybe<int>.FromNullable(0);

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(0);
    }

    #endregion

    #region Implicit Conversion

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void ImplicitConversion_FromValue_ReturnsSome(int value)
    {
        Maybe<int> maybe = value;

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_FromNull_ReturnsEmpty()
    {
        Maybe<string> maybe = (string?)null;

        maybe.HasValue.Should().BeFalse();
    }

    #endregion

    #region Default

    [Fact]
    public void Default_IsEmpty()
    {
        Maybe<int> maybe = default;

        maybe.HasValue.Should().BeFalse();
    }

    #endregion

    #region Non-Generic Factory

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void NonGenericFactory_Some_ReturnsSome(int value)
    {
        var maybe = Maybe.Some(value);

        maybe.Value.Should().Be(value);
    }

    [Fact]
    public void NonGenericFactory_Empty_ReturnsEmpty()
    {
        Maybe.Empty<int>().HasValue.Should().BeFalse();
    }

    [Fact]
    public void NonGenericFactory_FromNullable_Delegates()
    {
        Maybe.FromNullable<string>(null).HasValue.Should().BeFalse();
        Maybe.FromNullable("v").Value.Should().Be("v");
    }

    [Fact]
    public void StaticAbstractFactory_IsImplemented()
    {
        var some = Maybe<int>.Some(7);
        var empty = Maybe<int>.Empty();

        some.Value.Should().Be(7);
        empty.HasValue.Should().BeFalse();
    }

    #endregion
}