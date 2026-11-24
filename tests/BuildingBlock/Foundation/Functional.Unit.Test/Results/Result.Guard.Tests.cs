using BuildingBlock.Domain.Unit.Tests.Functional.Results;

namespace BuildingBlock.Functional.Unit.Test.Results;

public sealed class ResultGuardTests
{
    #region ErrorsRequired Tests

    [Fact]
    public void ErrorsRequired_WithNullErrors_ThrowsArgumentNullException()
    {
        Action act = () => ResultGuard.ErrorsRequired(null);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*result.errors.required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ErrorsRequired_WithErrors_DoesNotThrow(int errorCount)
    {
        var errors = Enumerable.Range(0, errorCount).Select(_ => ResultTestData.ErrorA).ToArray();

        Action act = () => ResultGuard.ErrorsRequired(errors);

        act.Should().NotThrow();
    }

    #endregion

    #region ErrorsNotEmpty Tests

    [Fact]
    public void ErrorsNotEmpty_WithNullErrors_ThrowsArgumentNullException()
    {
        Action act = () => ResultGuard.ErrorsNotEmpty(null);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*result.errors.required*");
    }

    [Fact]
    public void ErrorsNotEmpty_WithEmptyErrors_ThrowsArgumentException()
    {
        Action act = () => ResultGuard.ErrorsNotEmpty(Array.Empty<Error>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*result.errors.empty*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void ErrorsNotEmpty_WithErrors_DoesNotThrow(int errorCount)
    {
        var errors = Enumerable.Range(0, errorCount).Select(_ => ResultTestData.ErrorA).ToArray();

        Action act = () => ResultGuard.ErrorsNotEmpty(errors);

        act.Should().NotThrow();
    }

    #endregion

    #region ErrorsValid Tests

    [Fact]
    public void ErrorsValid_WithNullErrors_ThrowsArgumentNullException()
    {
        Action act = () => ResultGuard.ErrorsValid(null);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*result.errors.required*");
    }

    [Fact]
    public void ErrorsValid_WithEmptyErrors_ThrowsArgumentException()
    {
        Action act = () => ResultGuard.ErrorsValid(Array.Empty<Error>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*result.errors.empty*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ErrorsValid_WithDefaultError_ThrowsArgumentException(int errorCount)
    {
        var errors = new Error[errorCount];
        errors[^1] = default;

        Action act = () => ResultGuard.ErrorsValid(errors);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*result.errors.contains_null*");
    }

    [Fact]
    public void ErrorsValid_WithValidErrors_DoesNotThrow()
    {
        Action act = () => ResultGuard.ErrorsValid(
            new[] { ResultTestData.ErrorA, ResultTestData.ErrorB });

        act.Should().NotThrow();
    }

    #endregion

    #region ValueAccessible Tests

    [Fact]
    public void ValueAccessible_WithSuccess_DoesNotThrow()
    {
        Action act = () => ResultGuard.ValueAccessible(true);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValueAccessible_WithFailure_ThrowsInvalidOperationException()
    {
        Action act = () => ResultGuard.ValueAccessible(false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*result.value.invalid*");
    }

    #endregion

    #region ParameterRequired Tests

    [Fact]
    public void ParameterRequired_WithNullDelegate_ThrowsArgumentNullException()
    {
        Action act = () => ResultGuard.ParameterRequired((Delegate)null!, "param");

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*result.parameter.required*");
    }

    [Fact]
    public void ParameterRequired_WithNonNullDelegate_DoesNotThrow()
    {
        Delegate action = (Action)(() => { });
        ResultGuard.ParameterRequired(action, "param");
    }

    [Fact]
    public void ParameterRequired_WithNullClass_ThrowsArgumentNullException()
    {
        Action act = () => ResultGuard.ParameterRequired<string>(null!, "param");

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*result.parameter.required*");
    }

    [Fact]
    public void ParameterRequired_WithNonNullClass_DoesNotThrow()
    {
        ResultGuard.ParameterRequired("value", "param");
    }

    #endregion

    #region EnsureSuccess Tests

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    public void EnsureSuccess_WithSuccess_ReturnsValue(int value)
    {
        var result = ResultGuard.EnsureSuccess(true, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EnsureSuccess_WithFailure_ThrowsInvalidOperationException()
    {
        Action act = () => ResultGuard.EnsureSuccess(false, "value");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*result.value.invalid*");
    }

    #endregion
}
