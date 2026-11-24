namespace BuildingBlock.Domain.Unit.Tests.Functional.Metadata;

using BuildingBlock.Functional.Metadata;
using Xunit;

public sealed class MetadataConstantTests
{
    #region WellKnown Tests

    [Fact]
    public void WellKnown_Timestamp_HasExpectedValue()
    {
        MetadataConstant.WellKnown.Timestamp.Should().Be("timestamp");
    }

    [Fact]
    public void WellKnown_Timestamp_IsConstant()
    {
        var value = MetadataConstant.WellKnown.Timestamp;
        value.Should().Be("timestamp");
    }

    #endregion

    #region Constraints.Key Tests

    [Fact]
    public void Constraints_Key_Required_Value_IsTrue()
    {
        MetadataConstant.Constraints.Key.Required.Value.Should().Be(true);
    }

    [Fact]
    public void Constraints_Key_Required_Code_IsExpected()
    {
        MetadataConstant.Constraints.Key.Required.Code.Should().Be("metadata.key.required");
    }

    [Fact]
    public void Constraints_Key_Required_Message_IsExpected()
    {
        MetadataConstant.Constraints.Key.Required.Message.Should().Be("The metadata key is required.");
    }

    [Fact]
    public void Constraints_Key_MaxLength_Value_Is255()
    {
        MetadataConstant.Constraints.Key.MaxLength.Value.Should().Be(255);
    }

    [Fact]
    public void Constraints_Key_MaxLength_Code_IsExpected()
    {
        MetadataConstant.Constraints.Key.MaxLength.Code.Should().Be("metadata.key.max_length");
    }

    [Fact]
    public void Constraints_Key_MaxLength_Message_IsExpected()
    {
        MetadataConstant.Constraints.Key.MaxLength.Message.Should().Be("The maximum length for metadata key is 255.");
    }

    #endregion

    #region Constraints.Value Tests

    [Fact]
    public void Constraints_Value_Required_Value_IsTrue()
    {
        MetadataConstant.Constraints.Value.Required.Value.Should().Be(true);
    }

    [Fact]
    public void Constraints_Value_Required_Code_IsExpected()
    {
        MetadataConstant.Constraints.Value.Required.Code.Should().Be("metadata.value.required");
    }

    [Fact]
    public void Constraints_Value_Required_Message_IsExpected()
    {
        MetadataConstant.Constraints.Value.Required.Message.Should().Be("The metadata value is required.");
    }

    #endregion

    #region Constraints.Parameter Tests

    [Fact]
    public void Constraints_Parameter_Required_Value_IsTrue()
    {
        MetadataConstant.Constraints.Parameter.Required.Value.Should().Be(true);
    }

    [Theory]
    [InlineData("testParam", "metadata.testParam.required")]
    [InlineData("parameters", "metadata.parameters.required")]
    public void Constraints_Parameter_Required_Code_ReturnsExpected(string parameterName, string expectedCode)
    {
        MetadataConstant.Constraints.Parameter.Required.Code(parameterName).Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("testParam", "The parameter 'testParam' is required.")]
    [InlineData("parameters", "The parameter 'parameters' is required.")]
    public void Constraints_Parameter_Required_Message_ReturnsExpected(string parameterName, string expectedMessage)
    {
        MetadataConstant.Constraints.Parameter.Required.Message(parameterName).Should().Be(expectedMessage);
    }

    #endregion

    #region Defaults Tests

    [Fact]
    public void Defaults_ValueRequired_IsFalse()
    {
        MetadataConstant.Defaults.ValueRequired.Should().Be(false);
    }

    [Fact]
    public void Defaults_ValueNotNull_IsFalse()
    {
        MetadataConstant.Defaults.ValueNotNull.Should().Be(false);
    }

    #endregion
}