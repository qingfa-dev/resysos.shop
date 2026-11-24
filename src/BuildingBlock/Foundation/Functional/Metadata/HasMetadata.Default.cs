namespace BuildingBlock.Functional.Metadata;

public class DefaultHasMetadata : IHasMetadata
{
    public IMetadata Metadata { get;  set; } = DefaultMetadata.Empty();
}