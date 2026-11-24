using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;
using BuildingBlock.Domain.Modeling.Lifecycle.Modifiable;

namespace BuildingBlock.Domain.Entities;

/// <summary>
/// Marker concern for entities that keep creation and modification audit data.
/// Combines <see cref="ICreatable"/> and <see cref="IModifiable"/> so persistence
/// interceptors and audit policies can target a single interface.
/// </summary>
public interface IAuditable : ICreatable, IModifiable
{
}
