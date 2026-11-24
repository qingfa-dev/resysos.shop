namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultUnitTests
{
    [Fact]
    public void Unit_Value_IsDefault()
    {
        Unit.Value.Should().Be(default(Unit));
    }
}
