namespace BuildingBlock.Specification.Abstraction.Unit.Tests;

/// <summary>Tests for <see cref="SpecificationConstant"/>.</summary>
public sealed class SpecificationConstantTests
{
    #region Errors

    [Theory]
    [InlineData(0, "specification.paging.not_supported")]
    [InlineData(1, "specification.ordering.not_supported")]
    [InlineData(2, "specification.tracking.not_supported")]
    public void ShapingErrors_ShouldHaveDocumentedIdentity(int index, string code)
    {
        Error[] errors =
        [
            SpecificationConstant.Constraints.Paging.NotSupported.Error,
            SpecificationConstant.Constraints.Ordering.NotSupported.Error,
            SpecificationConstant.Constraints.Tracking.NotSupported.Error,
        ];

        errors[index].Code.Should().Be(code);
        errors[index].Type.Should().Be(ErrorType.NotSupported);
        errors[index].Message.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Messages

    [Fact]
    public void Messages_ShouldMatchDocumentedText()
    {
        SpecificationConstant.Constraints.Paging.NotSupported.Error.Message.Should().Be(
            "Cannot apply paging to a specification that is not a Specification implementation.");
        SpecificationConstant.Constraints.Ordering.NotSupported.Error.Message.Should().Be(
            "Cannot apply ordering to a specification that is not a Specification implementation.");
        SpecificationConstant.Constraints.Tracking.NotSupported.Error.Message.Should().Be(
            "Cannot disable tracking on a specification that is not a Specification implementation.");
    }

    #endregion

    #region Constraints

    [Fact]
    public void PagingConstraints_ShouldMatchDocumentedValues()
    {
        SpecificationConstant.Constraints.Paging.Page.Min.Value.Should().Be(1);
        SpecificationConstant.Constraints.Paging.PageSize.Min.Value.Should().Be(1);
        SpecificationConstant.Constraints.Paging.PageSize.Max.Value.Should().Be(1000);
    }

    [Fact]
    public void InvalidPagingErrors_ShouldMatchDocumentedIdentity()
    {
        var invalidPage = SpecificationConstant.Constraints.Paging.Page.Min.Error;
        var invalidPageSize = SpecificationConstant.Constraints.Paging.PageSize.Min.Error;

        invalidPage.Code.Should().Be("specification.paging.invalid_page");
        invalidPage.Type.Should().Be(ErrorType.Validation);
        invalidPage.Message.Should().Be("Page must be at least 1.");

        invalidPageSize.Code.Should().Be("specification.paging.invalid_page_size");
        invalidPageSize.Type.Should().Be(ErrorType.Validation);
        invalidPageSize.Message.Should().Be("Page size must be at least 1.");

        var tooLarge = SpecificationConstant.Constraints.Paging.PageSize.Max.Error;
        tooLarge.Code.Should().Be("specification.paging.invalid_page_size");
        tooLarge.Message.Should().Be("Page size cannot exceed 1000.");
    }

    [Fact]
    public void OrderingKeySelectorError_ShouldMatchDocumentedIdentity()
    {
        var error = SpecificationConstant.Constraints.Ordering.KeySelector.Required.Error;

        error.Code.Should().Be("specification.ordering.key_selector.required");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Be("An ordering key selector is required.");
    }

    #endregion

    #region Required Errors

    [Fact]
    public void RequiredSpecificationError_ShouldMatchDocumentedIdentity()
    {
        var required = SpecificationConstant.Constraints.Specification.Required.Error;

        required.Code.Should().Be("specification.required");
        required.Type.Should().Be(ErrorType.Validation);
        required.Message.Should().Be("A specification instance is required.");
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        SpecificationConstant.Defaults.Page.Should().Be(1);
        SpecificationConstant.Defaults.PageSize.Should().Be(50);
        SpecificationConstant.Defaults.Descending.Should().BeFalse();
        SpecificationConstant.Defaults.TrackingEnabled.Should().BeTrue();
    }

    #endregion

    #region Factory Inputs

    [Fact]
    public void FactoryConstraintErrors_ShouldMatchDocumentedIdentity()
    {
        Error[] errors =
        [
            SpecificationConstant.Constraints.Criteria.Required.Error,
            SpecificationConstant.Constraints.Id.Required.Error,
            SpecificationConstant.Constraints.Ids.Required.Error,
        ];

        errors[0].Code.Should().Be("specification.criteria.required");
        errors[1].Code.Should().Be("specification.id.required");
        errors[2].Code.Should().Be("specification.ids.required");
        errors.Should().OnlyContain(error => error.Type == ErrorType.Validation);
    }

    #endregion
}
