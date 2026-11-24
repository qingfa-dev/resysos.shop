namespace BuildingBlock.Specification.Unit.Tests;

/// <summary>Tests for <see cref="ResultTerminalExtensions"/>.</summary>
public sealed class ResultTerminalExtensionTests
{
    [Fact]
    #region Terminal

    public void OrThrow_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Success(42);

        result.OrThrow().Should().Be(42);
    }

    [Fact]
    public void OrThrow_OnFailure_ShouldThrowAtTheEnd()
    {
        var result = Result.Failure<int>(SpecificationConstant.Constraints.Paging.NotSupported.Error);

        var act = () => result.OrThrow();

        act.Should().Throw<Exception>()
            .Which.Message.Should().Contain(
                SpecificationConstant.Constraints.Paging.NotSupported.Error.Code)
            .And.Contain(SpecificationConstant.Constraints.Paging.NotSupported.Error.Message);
    }

    [Fact]
    public void OrThrow_ShouldUnwrapFailedQueryShaping()
    {
        ISpecification<Person> specification = new BareSpecification();

        var shapable = specification.WithPaging(1, 10);

        shapable.IsFailure.Should().BeTrue();
        var act = () => shapable.OrThrow();
        act.Should().Throw<Exception>();
    }
    #endregion
}
