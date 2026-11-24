using BuildingBlock.Domain.Modeling.Foundation.Versionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Foundation.Versionable;

/// <summary>Specifications for <see cref="VersionableConstant"/> covering constant values and errors.</summary>
public sealed class VersionableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        VersionableConstant.Entity.Required.Error.Code.Should().Be("versionable.entity.required");
        VersionableConstant.Version.Min.Error.Code.Should().Be("versionable.version.negative");
        VersionableConstant.Version.Max.Error.Code.Should().Be("versionable.version.overflow");
        VersionableConstant.Version.Range.Error.Code.Should().Be("versionable.version.out_of_range");
    }

    [Fact]
    public void Constants_ShouldMatchNestedDefinitions()
    {
        VersionableConstant.Version.Min.Value.Should().Be(0L);
        VersionableConstant.Version.Max.Value.Should().Be(long.MaxValue);
        VersionableConstant.Defaults.InitialVersion.Should().Be(0L);
        VersionableConstant.Defaults.Increment.Should().Be(1L);
    }

    [Fact]
    public void Concurrency_ShouldDescribeExpectedAndActualVersions()
    {
        var error = VersionableConstant.Version.Conflict.Concurrency(expected: 3, actual: 5);

        error.Code.Should().Be("versionable.version.concurrency_conflict");
        error.Type.Should().Be(ErrorType.Conflict);
        error.Message.Should().Contain("3").And.Contain("5");
    }
}
