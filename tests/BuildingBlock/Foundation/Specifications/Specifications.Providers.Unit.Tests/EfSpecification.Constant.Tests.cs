namespace BuildingBlock.Specifications.Providers.Unit.Tests;

/// <summary>Tests for <see cref="EfSpecificationConstant"/>.</summary>
public sealed class EfSpecificationConstantTests
{
    #region Errors

    [Theory]
    [InlineData(0, "ef_specification.query.required")]
    [InlineData(1, "ef_specification.specification.required")]
    [InlineData(2, "ef_specification.evaluator.required")]
    [InlineData(3, "ef_specification.options.required")]
    [InlineData(4, "ef_specification.services.required")]
    public void ProviderErrors_ShouldHaveDocumentedIdentity(int index, string code)
    {
        Error[] errors =
        [
            EfSpecificationConstant.Query.Required.Error,
            EfSpecificationConstant.Specification.Required.Error,
            EfSpecificationConstant.Evaluator.Required.Error,
            EfSpecificationConstant.Options.Required.Error,
            EfSpecificationConstant.Services.Required.Error,
        ];

        errors[index].Code.Should().Be(code);
        errors[index].Type.Should().Be(ErrorType.Validation);
        errors[index].Message.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        EfSpecificationConstant.Defaults.UseSplitQuery.Should().BeFalse();
        EfSpecificationConstant.Defaults.IgnoreQueryFilters.Should().BeFalse();
    }

    #endregion

    #region Options

    [Fact]
    public void Options_ShouldUseDefaultsAndDocumentedSection()
    {
        EfSpecificationOptions.SectionName
            .Should().Be("BuildingBlocks:Domain:Specifications:EfCore");

        var options = new EfSpecificationOptions();
        options.UseSplitQuery.Should().Be(EfSpecificationConstant.Defaults.UseSplitQuery);
        options.IgnoreQueryFilters.Should().Be(EfSpecificationConstant.Defaults.IgnoreQueryFilters);
    }

    #endregion
}
