using BuildingBlock.Domain.SmartEnums;

namespace BuildingBlock.Domain.Unit.Test.SmartEnums;

/// <summary>Specifications for <see cref="SmartEnum{TEnum}"/> covering registration, lookup, and equality.</summary>
public sealed class SmartEnumSpec
{
    #region Helper Types

    private sealed class OrderStatus : SmartEnum<OrderStatus>
    {
        public static readonly OrderStatus Pending = new(2, "Pending");
        public static readonly OrderStatus Draft = new(1, "Draft");

        private OrderStatus(int id, string name) : base(id, name) { }
    }

    private sealed class Probe : SmartEnum<Probe>
    {
        private Probe(int id, string name) : base(id, name) { }

        public static Probe Create(int id, string name) => new(id, name);
    }

    private sealed class DuplicateIds : SmartEnum<DuplicateIds>
    {
        public static readonly DuplicateIds First = new(1, "First");
        public static readonly DuplicateIds Second = new(1, "Second");

        private DuplicateIds(int id, string name) : base(id, name) { }
    }

    private sealed class DuplicateNames : SmartEnum<DuplicateNames>
    {
        public static readonly DuplicateNames First = new(1, "Same");
        public static readonly DuplicateNames Second = new(2, "same");

        private DuplicateNames(int id, string name) : base(id, name) { }
    }

    #endregion

    #region Registration

    [Fact]
    public void GetAll_ShouldReturnMembersOrderedById()
    {
        var members = OrderStatus.GetAll();

        members.Should().HaveCount(2);
        members[0].Should().BeSameAs(OrderStatus.Draft);
        members[1].Should().BeSameAs(OrderStatus.Pending);
    }

    [Fact]
    public void GetAll_ShouldReturnStableList()
    {
        OrderStatus.GetAll().Should().BeSameAs(OrderStatus.GetAll());
    }

    [Fact]
    public void GetAll_DuplicateIds_ShouldThrow()
    {
        var act = () => DuplicateIds.GetAll();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*duplicate smart enum id*");
    }

    [Fact]
    public void GetAll_DuplicateNames_ShouldThrow()
    {
        var act = () => DuplicateNames.GetAll();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*duplicate smart enum name*");
    }

    #endregion

    #region Construction

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var member = Probe.Create(1, "  Ready  ");

        member.Name.Should().Be("Ready");
        member.Id.Should().Be(1);
    }

    [Fact]
    public void Constructor_NegativeId_ShouldThrow()
    {
        var act = () => Probe.Create(-1, "Ready");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_MissingName_ShouldThrow(string? name)
    {
        var act = () => Probe.Create(1, name!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NameTooLong_ShouldThrow()
    {
        var name = new string('a', SmartEnumConstant.Name.MaxLength.Value + 1);

        var act = () => Probe.Create(1, name);

        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Lookup

    [Fact]
    public void TryFromId_KnownId_ShouldReturnMember()
    {
        var result = OrderStatus.TryFromId(2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(OrderStatus.Pending);
    }

    [Fact]
    public void TryFromId_UnknownId_ShouldFailWithNotFound()
    {
        var result = OrderStatus.TryFromId(99);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("smart_enum.id.not_found");
    }

    [Fact]
    public void FromId_KnownId_ShouldReturnMember()
    {
        OrderStatus.FromId(1).Should().BeSameAs(OrderStatus.Draft);
    }

    [Fact]
    public void FromId_UnknownId_ShouldThrow()
    {
        var act = () => OrderStatus.FromId(99);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("pending")]
    [InlineData("  PENDING  ")]
    public void TryFromName_ShouldIgnoreCaseAndWhitespace(string name)
    {
        var result = OrderStatus.TryFromName(name);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(OrderStatus.Pending);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Unknown")]
    public void TryFromName_InvalidName_ShouldFailWithNotFound(string? name)
    {
        var result = OrderStatus.TryFromName(name);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("smart_enum.name.not_found");
    }

    [Fact]
    public void FromName_KnownName_ShouldReturnMember()
    {
        OrderStatus.FromName("draft").Should().BeSameAs(OrderStatus.Draft);
    }

    [Fact]
    public void FromName_UnknownName_ShouldThrow()
    {
        var act = () => OrderStatus.FromName("Unknown");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Equality and formatting

    [Fact]
    public void Equals_SameMember_ShouldBeEqual()
    {
        var left = OrderStatus.FromId(1);
        var right = OrderStatus.FromName("Draft");

        left.Should().Be(right);
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentMember_ShouldNotBeEqual()
    {
        var draft = OrderStatus.Draft;

        draft.Equals(OrderStatus.Pending).Should().BeFalse();
        (draft == OrderStatus.Pending).Should().BeFalse();
        (draft != OrderStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public void CompareTo_ShouldCompareById()
    {
        OrderStatus.Draft.CompareTo(OrderStatus.Pending).Should().BeNegative();
        OrderStatus.Pending.CompareTo(OrderStatus.Draft).Should().BePositive();
        OrderStatus.Draft.CompareTo(OrderStatus.Draft).Should().Be(0);
        OrderStatus.Draft.CompareTo(null).Should().BePositive();
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        OrderStatus.Pending.ToString().Should().Be("Pending");
    }

    #endregion
}
