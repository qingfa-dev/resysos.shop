using SharedTestData = Testing.Data.TestData;

namespace BuildingBlock.Persistence.Abstractions.Unit.Tests;

#region Aggregates

internal sealed class Order : IIdentifiable<Guid>
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Amount { get; init; }
}

#endregion

#region Data

internal static class Orders
{
    public static Order New(string name, int amount)
        => new() { Id = SharedTestData.Id(), Name = name, Amount = amount };

    public static Order WithId(Guid id, string name, int amount)
        => new() { Id = id, Name = name, Amount = amount };
}

#endregion
