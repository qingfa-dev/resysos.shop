namespace BuildingBlock.Domain.Unit.Tests.Functional.Metadata;

using BuildingBlock.Functional.Metadata;

public sealed class DefaultHasMetadataTests
{
    [Fact]
    public void Constructor_InitializesWithEmptyMetadata()
    {
        var entity = new DefaultHasMetadata();

        entity.Metadata.Should().NotBeNull();
        entity.Metadata.HasMetadata().Should().Be(false);
        entity.Metadata.Count.Should().Be(0);
    }

    [Fact]
    public void MetadataProperty_CanBeSet()
    {
        var entity = new DefaultHasMetadata();
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        entity.Metadata = metadata;

        entity.Metadata.Should().BeSameAs(metadata);
    }

    [Fact]
    public void MetadataProperty_CanBeSetToNull()
    {
        var entity = new DefaultHasMetadata
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        entity.Metadata = null!;

        entity.Metadata.Should().BeNull();
    }

    [Fact]
    public void ImplementsIHasMetadata()
    {
        var entity = new DefaultHasMetadata();

        IHasMetadata hasMetadata = entity;

        hasMetadata.Should().BeSameAs(entity);
        hasMetadata.Metadata.Should().BeSameAs(entity.Metadata);
    }

    [Fact]
    public void MultipleInstances_HaveIndependentMetadata()
    {
        var entity1 = new DefaultHasMetadata();
        var entity2 = new DefaultHasMetadata();

        entity1.Metadata.WithMetadata("key1", "value1");
        entity2.Metadata.WithMetadata("key2", "value2");

        entity1.Metadata.Count.Should().Be(1);
        entity1.Metadata.GetMetadata("key1").Should().Be("value1");
        entity1.Metadata.HasMetadata("key2").Should().Be(false);

        entity2.Metadata.Count.Should().Be(1);
        entity2.Metadata.GetMetadata("key2").Should().Be("value2");
        entity2.Metadata.HasMetadata("key1").Should().Be(false);
    }

    [Fact]
    public void Extensions_AllMethodsWork()
    {
        var entity = new DefaultHasMetadata();

        entity.HasMetadata().Should().Be(false);
        entity.WithMetadata("key", "value");
        entity.HasMetadata().Should().Be(true);
        entity.HasMetadata("key").Should().Be(true);
        entity.GetMetadata("key").Should().Be("value");
        entity.GetMetadata<int>("key").Should().Be(default);
        entity.TryGetMetadata("key", out var value).Should().Be(true);
        value.Should().Be("value");
        entity.WithoutMetadata("key");
        entity.HasMetadata("key").Should().Be(false);
    }
}