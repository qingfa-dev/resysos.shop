using BuildingBlock.Domain.Entities;
using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;
using BuildingBlock.Domain.Modeling.Lifecycle.Modifiable;

namespace BuildingBlock.Domain.Unit.Test.Entities;

/// <summary>Tests for <see cref="IAuditable"/>.</summary>
public sealed class AuditableInterfaceTests
{
    #region Helper Types

    private sealed class Order : Entity<Guid>
    {
        public Order(Guid id) : base(id) { }
    }

    private sealed class Invoice : Entity<Guid>
    {
        public Invoice(Guid id) : base(id) { }
    }

    private sealed class Line : Entity<int>
    {
        public Line(int id) : base(id) { }
    }

    private sealed class AuditedOrder : Entity<Guid>, IAuditable
    {
        public AuditedOrder(Guid id) : base(id) { }

        public DateTimeOffset CreatedAtUtc { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset? ModifiedAtUtc { get; set; }

        public string? ModifiedBy { get; set; }
    }

    #endregion

    [Fact]
    public void Auditable_ShouldExposeCreationAndModificationState()
    {
        var entity = new AuditedOrder(Guid.NewGuid());
        IAuditable auditable = entity;

        auditable.Should().BeAssignableTo<ICreatable>();
        auditable.Should().BeAssignableTo<IModifiable>();

        var created = DateTimeOffset.UtcNow;
        var modified = created.AddMinutes(5);

        auditable.CreatedAtUtc = created;
        auditable.CreatedBy = "creator";
        auditable.ModifiedAtUtc = modified;
        auditable.ModifiedBy = "modifier";

        entity.CreatedAtUtc.Should().Be(created);
        entity.CreatedBy.Should().Be("creator");
        entity.ModifiedAtUtc.Should().Be(modified);
        entity.ModifiedBy.Should().Be("modifier");
    }
}
