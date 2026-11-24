namespace BuildingBlock.Domain.Unit.Test.Functional.Optionals;

using BuildingBlock.Functional.Optionals;
using BuildingBlock.Functional.Errors;
using Xunit;

public sealed class MaybeConstantTests
{
    #region Constraints.Value Tests

    [Fact]
    public void Constraints_Value_Required_IsErrorValidation()
    {
        MaybeConstant.Constraints.Value.Required.Should().BeOfType<Error>();
        MaybeConstant.Constraints.Value.Required.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Constraints_Value_Required_Code_IsExpected()
    {
        MaybeConstant.Constraints.Value.Required.Code.Should().Be("maybe.value.required");
    }

    [Fact]
    public void Constraints_Value_Required_Message_IsExpected()
    {
        MaybeConstant.Constraints.Value.Required.Message.Should().Be("The Maybe value is required but was None.");
    }

    [Fact]
    public void Constraints_Value_Required_ToString_ContainsCodeAndMessage()
    {
        var str = MaybeConstant.Constraints.Value.Required.ToString();
        str.Should().Contain("maybe.value.required");
        str.Should().Contain("The Maybe value is required but was None.");
    }

    #endregion

    #region Constraints.Parameter Tests

    [Fact]
    public void Constraints_Parameter_Required_IsErrorValidation()
    {
        MaybeConstant.Constraints.Parameter.Required.Should().BeOfType<Error>();
        MaybeConstant.Constraints.Parameter.Required.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Constraints_Parameter_Required_Code_IsExpected()
    {
        MaybeConstant.Constraints.Parameter.Required.Code.Should().Be("maybe.parameter.required");
    }

    [Fact]
    public void Constraints_Parameter_Required_Message_IsExpected()
    {
        MaybeConstant.Constraints.Parameter.Required.Message.Should().Be("The parameter is required but was null.");
    }

    [Fact]
    public void Constraints_Parameter_Required_ToString_ContainsCodeAndMessage()
    {
        var str = MaybeConstant.Constraints.Parameter.Required.ToString();
        str.Should().Contain("maybe.parameter.required");
        str.Should().Contain("The parameter is required but was null.");
    }

    #endregion
}