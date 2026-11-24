namespace BuildingBlock.Domain.Unit.Tests.Functional.Metadata;

using BuildingBlock.Functional.Metadata;
using Xunit;

public sealed class HasMetadataExtensionsTests
{
    private sealed class TestEntity : IHasMetadata
    {
        public IMetadata Metadata { get; set; } = DefaultMetadata.Empty();
    }

    #region Presence Tests

    [Theory]
    [InlineData(true, "non-empty")]
    [InlineData(false, "empty")]
    public void HasMetadata_NoArgs_ReturnsExpected(bool hasEntries, string _)
    {
        var entity = new TestEntity
        {
            Metadata = hasEntries
                ? DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
                : DefaultMetadata.Empty()
        };

        var result = entity.HasMetadata();

        result.Should().Be(hasEntries);
    }

    [Fact]
    public void HasMetadata_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.HasMetadata((IHasMetadata)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("key", true, "existing key")]
    [InlineData("nonexistent", false, "non-existing key")]
    public void HasMetadata_WithKey_ReturnsExpected(string key, bool expected, string _)
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        var result = entity.HasMetadata(key);

        result.Should().Be(expected);
    }

    [Fact]
    public void HasMetadata_WithKey_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.HasMetadata((IHasMetadata)null!, "key");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("key", true, "existing key")]
    [InlineData("nonexistent", false, "non-existing key")]
    public void HasMetadata_WithOutParameter_ReturnsExpected(string key, bool expected, string _)
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        var result = entity.HasMetadata(key, out var value);

        result.Should().Be(expected);
        if (expected) value.Should().Be("value");
        else value.Should().BeNull();
    }

    [Fact]
    public void HasMetadata_WithOutParameter_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.HasMetadata((IHasMetadata)null!, "key", out _);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    [InlineData("valid", "valid key")]
    public void HasMetadata_WithKey_ValidatesKey(string? key, string _)
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        if (string.IsNullOrWhiteSpace(key))
        {
            Action act = () => entity.HasMetadata(key!);
            act.Should().Throw<ArgumentException>();
        }
        else
        {
            var result = entity.HasMetadata(key!);
            result.Should().Be(key == "key");
        }
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void HasMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var entity = new TestEntity
        {
            Metadata = shouldSucceed
                ? DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"))
                : DefaultMetadata.Empty()
        };

        if (shouldSucceed)
        {
            entity.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => entity.HasMetadata(key);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region Retrieval Tests

    [Fact]
    public void GetMetadata_NoArgs_ReturnsMetadata()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        var entity = new TestEntity { Metadata = metadata };

        entity.GetMetadata().Should().BeSameAs(metadata);
    }

    [Fact]
    public void GetMetadata_NoArgs_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.GetMetadata((IHasMetadata)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("key", "value", "existing key")]
    [InlineData("nonexistent", null, "non-existing key")]
    public void GetMetadata_WithKey_ReturnsExpected(string key, string? expected, string _)
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        var result = entity.GetMetadata(key);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetMetadata_WithKey_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.GetMetadata((IHasMetadata)null!, "key");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetMetadata_Generic_WithExistingKeyAndMatchingType_ReturnsTypedValue()
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", 42))
        };

        entity.GetMetadata<int>("key").Should().Be(42);
    }

    [Fact]
    public void GetMetadata_Generic_WithExistingKeyAndNonMatchingType_ReturnsDefault()
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "not an int"))
        };

        entity.GetMetadata<int>("key").Should().Be(default);
    }

    [Fact]
    public void GetMetadata_Generic_WithNonExistingKey_ReturnsDefault()
    {
        var entity = new TestEntity();
        entity.GetMetadata<int>("nonexistent").Should().Be(default);
    }

    [Fact]
    public void GetMetadata_Generic_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.GetMetadata<int>((IHasMetadata)null!, "key");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("key", true, "value", "existing key")]
    [InlineData("nonexistent", false, null, "non-existing key")]
    public void TryGetMetadata_WithKey_ReturnsExpected(string key, bool expected, string? value, string _)
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        var result = entity.TryGetMetadata(key, out var outValue);

        result.Should().Be(expected);
        outValue.Should().Be(value);
    }

    [Fact]
    public void TryGetMetadata_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.TryGetMetadata((IHasMetadata)null!, "key", out _);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryGetMetadata_Generic_WithExistingKeyAndMatchingType_ReturnsTrueAndSetsValue()
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", 42))
        };

        var result = entity.TryGetMetadata("key", out int outValue);

        result.Should().Be(true);
        outValue.Should().Be(42);
    }

    [Fact]
    public void TryGetMetadata_Generic_WithExistingKeyAndNonMatchingType_ReturnsFalseAndSetsDefault()
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "not an int"))
        };

        var result = entity.TryGetMetadata("key", out int outValue);

        result.Should().Be(false);
        outValue.Should().Be(default);
    }

    [Fact]
    public void TryGetMetadata_Generic_WithNonExistingKey_ReturnsFalseAndSetsDefault()
    {
        var entity = new TestEntity();
        var result = entity.TryGetMetadata("nonexistent", out int outValue);
        result.Should().Be(false);
        outValue.Should().Be(default);
    }

    [Fact]
    public void TryGetMetadata_Generic_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.TryGetMetadata<int>((IHasMetadata)null!, "key", out _);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void GetMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var entity = new TestEntity
        {
            Metadata = shouldSucceed
                ? DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"))
                : DefaultMetadata.Empty()
        };

        if (shouldSucceed)
        {
            entity.GetMetadata(key).Should().Be("value");
        }
        else
        {
            Action act = () => entity.GetMetadata(key);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void TryGetMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var entity = new TestEntity
        {
            Metadata = shouldSucceed
                ? DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"))
                : DefaultMetadata.Empty()
        };

        if (shouldSucceed)
        {
            entity.TryGetMetadata(key, out var value).Should().Be(true);
            value.Should().Be("value");
        }
        else
        {
            Action act = () => entity.TryGetMetadata(key, out object? _);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region Composition Tests

    [Fact]
    public void WithMetadata_SingleKeyValue_AddsOrUpdatesEntry()
    {
        var entity = new TestEntity();
        var result = entity.WithMetadata("key", "value");

        result.Should().BeSameAs(entity);
        entity.Metadata.HasMetadata("key").Should().Be(true);
        entity.Metadata.GetMetadata("key").Should().Be("value");
    }

    [Fact]
    public void WithMetadata_SingleKeyValue_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.WithMetadata((TestEntity)null!, "key", "value");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void WithMetadata_SingleKeyValue_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        var entity = new TestEntity();

        Action act = () => entity.WithMetadata(key!, "value");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithMetadata_SingleKeyValue_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var entity = new TestEntity();

        if (shouldSucceed)
        {
            var result = entity.WithMetadata(key, "value");
            result.Should().BeSameAs(entity);
            entity.Metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => entity.WithMetadata(key, "value");
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Fact]
    public void WithMetadata_IMetadata_MergesEntries()
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key1", "value1"))
        };
        var other = DefaultMetadata.Create(new KeyValuePair<string, object>("key2", "value2"));

        var result = entity.WithMetadata(other);

        result.Should().BeSameAs(entity);
        entity.Metadata.Count.Should().Be(2);
        entity.Metadata.GetMetadata("key1").Should().Be("value1");
        entity.Metadata.GetMetadata("key2").Should().Be("value2");
    }

    [Fact]
    public void WithMetadata_IMetadata_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.WithMetadata((TestEntity)null!, DefaultMetadata.Empty());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithMetadata_IMetadata_WithNullMetadata_ThrowsArgumentNullException()
    {
        var entity = new TestEntity();

        // ParameterNotNull now validates the value
        Action act = () => entity.WithMetadata((IMetadata)null!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*metadata.metadata.required*");
    }

    [Fact]
    public void WithMetadata_Parameters_AddsAllEntries()
    {
        var entity = new TestEntity();
        var parameters = new[]
        {
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"),
        };

        var result = entity.WithMetadata(parameters);

        result.Should().BeSameAs(entity);
        entity.Metadata.Count.Should().Be(2);
        entity.Metadata.GetMetadata("key1").Should().Be("value1");
        entity.Metadata.GetMetadata("key2").Should().Be("value2");
    }

    [Fact]
    public void WithMetadata_Parameters_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.WithMetadata((TestEntity)null!, Array.Empty<KeyValuePair<string, object>>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithMetadata_IMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var entity = new TestEntity();

        if (shouldSucceed)
        {
            var other = DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"));
            entity.WithMetadata(other);
            entity.Metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            // Key validation now happens at creation time in Create/From methods
            Action act = () => DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"));
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithMetadata_Parameters_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var parameters = new[] { new KeyValuePair<string, object>(key, "value") };
        var entity = new TestEntity();

        if (shouldSucceed)
        {
            entity.WithMetadata(parameters);
            entity.Metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => entity.WithMetadata(parameters);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region Removal Tests

    [Fact]
    public void WithoutMetadata_NoArgs_ClearsAllEntries()
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(
                new KeyValuePair<string, object>("key1", "value1"),
                new KeyValuePair<string, object>("key2", "value2"))
        };

        var result = entity.WithoutMetadata();

        result.Should().BeSameAs(entity);
        entity.Metadata.HasMetadata().Should().Be(false);
        entity.Metadata.Count.Should().Be(0);
    }

    [Fact]
    public void WithoutMetadata_NoArgs_Generic_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.WithoutMetadata((TestEntity)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithoutMetadata_WithKey_RemovesEntry()
    {
        var entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(
                new KeyValuePair<string, object>("key1", "value1"),
                new KeyValuePair<string, object>("key2", "value2"))
        };

        var result = entity.WithoutMetadata("key1");

        result.Should().BeSameAs(entity);
        entity.Metadata.Count.Should().Be(1);
        entity.Metadata.HasMetadata("key1").Should().Be(false);
        entity.Metadata.HasMetadata("key2").Should().Be(true);
    }

    [Fact]
    public void WithoutMetadata_WithKey_Generic_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.WithoutMetadata((TestEntity)null!, "key");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void WithoutMetadata_WithKey_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        var entity = new TestEntity { Metadata = DefaultMetadata.Empty() };

        Action act = () => entity.WithoutMetadata(key!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithoutMetadata_WithKey_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var entity = new TestEntity
        {
            Metadata = shouldSucceed
                ? DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"))
                : DefaultMetadata.Empty()
        };

        if (shouldSucceed)
        {
            var result = entity.WithoutMetadata(key);
            result.Should().BeSameAs(entity);
            entity.Metadata.HasMetadata(key).Should().Be(false);
        }
        else
        {
            Action act = () => entity.WithoutMetadata(key);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region Absence Tests

    [Theory]
    [InlineData(true, "empty")]
    [InlineData(false, "non-empty")]
    public void WithoutMetadata_NoArgs_ReturnsExpected(bool empty, string _)
    {
        IHasMetadata entity = new TestEntity
        {
            Metadata = empty ? DefaultMetadata.Empty() : DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        var result = entity.WithoutMetadata();

        result.Should().Be(empty);
    }

    [Fact]
    public void WithoutMetadata_NoArgs_IHasMetadata_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.WithoutMetadata((IHasMetadata)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("nonexistent", true, "non-existing key")]
    [InlineData("key", false, "existing key")]
    public void WithoutMetadata_WithKey_ReturnsExpected(string key, bool expected, string _)
    {
        IHasMetadata entity = new TestEntity
        {
            Metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"))
        };

        var result = entity.WithoutMetadata(key);

        result.Should().Be(expected);
    }

    [Fact]
    public void WithoutMetadata_WithKey_IHasMetadata_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => HasMetadataExtensions.WithoutMetadata((IHasMetadata)null!, "key");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void WithoutMetadata_WithKey_Absence_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        IHasMetadata entity = new TestEntity { Metadata = DefaultMetadata.Empty() };

        Action act = () => entity.WithoutMetadata(key!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithoutMetadata_WithKey_Absence_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        IHasMetadata entity = new TestEntity
        {
            Metadata = shouldSucceed
                ? DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"))
                : DefaultMetadata.Empty()
        };

        if (shouldSucceed)
        {
            // Use concrete type to invoke generic extension that removes the key
            var concreteEntity = (TestEntity)entity;
            concreteEntity.WithoutMetadata(key).Should().BeSameAs(concreteEntity);
            entity.Metadata.HasMetadata(key).Should().Be(false);
        }
        else
        {
            Action act = () => entity.WithoutMetadata(key);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion
}