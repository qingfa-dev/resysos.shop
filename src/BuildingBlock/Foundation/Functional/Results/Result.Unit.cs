namespace BuildingBlock.Functional.Results;

/// <summary>Represents successful completion when no meaningful value is returned.</summary>
public readonly record struct Unit
{
    public static Unit Value => default;
}

