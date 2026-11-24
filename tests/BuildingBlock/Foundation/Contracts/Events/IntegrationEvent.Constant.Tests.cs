namespace BuildingBlock.Contracts.Unit.Tests.Events;

/// <summary>Tests for <see cref="IntegrationEventConstant"/>.</summary>
public sealed class IntegrationEventConstantTests
{
    #region Errors

    [Theory]
    [InlineData(0, "integration_event.version.invalid")]
    [InlineData(1, "integration_event.type.required")]
    [InlineData(2, "integration_event.metadata.key.required")]
    public void ConstraintErrors_ShouldHaveDocumentedIdentity(int index, string code)
    {
        Error[] errors =
        [
            IntegrationEventConstant.Constraints.Version.Min.Error,
            IntegrationEventConstant.Constraints.EventType.Required.Error,
            IntegrationEventConstant.Constraints.Metadata.Key.Required.Error,
        ];

        errors[index].Code.Should().Be(code);
        errors[index].Type.Should().Be(ErrorType.Validation);
        errors[index].Message.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Metadata Keys

    [Fact]
    public void MetadataKeys_ShouldMatchDocumentedValues()
    {
        IntegrationEventConstant.MetadataKeys.CorrelationId.Should().Be("CorrelationId");
        IntegrationEventConstant.MetadataKeys.CausationId.Should().Be("CausationId");
        IntegrationEventConstant.MetadataKeys.UserId.Should().Be("UserId");
        IntegrationEventConstant.MetadataKeys.TenantId.Should().Be("TenantId");
        IntegrationEventConstant.MetadataKeys.TraceId.Should().Be("TraceId");
        IntegrationEventConstant.MetadataKeys.SpanId.Should().Be("SpanId");
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        IntegrationEventConstant.Defaults.Version.Should().Be(1);
        IntegrationEventConstant.Constraints.Version.Min.Value.Should().Be(1);
    }

    #endregion
}
