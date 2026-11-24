namespace BuildingBlock.Domain.Unit.Tests.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using BuildingBlock.Functional.Errors;
using Xunit;

public sealed class MaybeGuardTests
{
    #region ValueRequired Tests

    [Fact]
    public void ValueRequired_WithNullClass_ThrowsArgumentNullException()
    {
        Action act = () => MaybeGuard.ValueRequired<string>(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*maybe.value.required*");
    }

    [Fact]
    public void ValueRequired_WithNonNullClass_DoesNotThrow()
    {
        MaybeGuard.ValueRequired("value");
    }

    [Fact]
    public void ValueRequired_WithNullStruct_ThrowsArgumentNullException()
    {
        int? value = null;
        Action act = () => MaybeGuard.ValueRequired(value);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*maybe.value.required*");
    }

    [Fact]
    public void ValueRequired_WithNonNullStruct_DoesNotThrow()
    {
        MaybeGuard.ValueRequired((int?)42);
    }

    #endregion

    #region ParameterRequired Tests (Delegate)

    [Fact]
    public void ParameterRequired_WithNullDelegate_ThrowsArgumentNullException()
    {
        Action act = () => MaybeGuard.ParameterRequired((Delegate)null!, "param");
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*maybe.parameter.required*");
    }

    [Fact]
    public void ParameterRequired_WithNonNullDelegate_ReturnsNormally()
    {
        Delegate action = (Action)(() => { });
        MaybeGuard.ParameterRequired(action, "param");
    }

    [Fact]
    public void ParameterRequired_WithNullAction_ThrowsArgumentNullException()
    {
        Action act = () => MaybeGuard.ParameterRequired((Action)null!, "param");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ParameterRequired_WithNullFunc_ThrowsArgumentNullException()
    {
        Action act = () => MaybeGuard.ParameterRequired((Func<int, int>)null!, "param");
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}