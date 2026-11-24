namespace BuildingBlock.Domain.Unit.Tests.Functional.Metadata;

using System.Collections.Generic;
using BuildingBlock.Functional.Metadata;
using Xunit;

public sealed class DefaultMetadataTests
{
    #region Factory Tests

    [Fact]
    public void Empty_ReturnsEmptyMetadata()
    {
        var metadata = DefaultMetadata.Empty();

        metadata.Should().NotBeNull();
        metadata.HasMetadata().Should().Be(false);
        metadata.Count.Should().Be(0);
    }

    [Fact]
    public void Create_WithParameters_ReturnsMetadataWithEntries()
    {
        var parameters = new[]
        {
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", 42),
        };

        var metadata = DefaultMetadata.Create(parameters);

        metadata.Should().NotBeNull();
        metadata.HasMetadata().Should().Be(true);
        metadata.Count.Should().Be(2);
        metadata.GetMetadata("key1").Should().Be("value1");
        metadata.GetMetadata("key2").Should().Be(42);
    }

    [Fact]
    public void Create_WithNullParameters_ThrowsArgumentNullException()
    {
        // ParameterRequired checks the parameter NAME "parameters", not the value
        // But the underlying Dictionary constructor throws when null is passed
        Action act = () => DefaultMetadata.Create(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithEmptyParameters_ThrowsArgumentException()
    {
        Action act = () => DefaultMetadata.Create(Array.Empty<KeyValuePair<string, object>>());
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.parameters.not_empty*");
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void Create_WithKeyLength_ValidatesAtCreationTime(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var parameters = new[] { new KeyValuePair<string, object>(key, "value") };

        if (shouldSucceed)
        {
            var metadata = DefaultMetadata.Create(parameters);
            metadata.Should().NotBeNull();
            metadata.Count.Should().Be(1);
            metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => DefaultMetadata.Create(parameters);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Fact]
    public void From_WithDictionary_ReturnsMetadataWithEntries()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42,
        };

        var metadata = DefaultMetadata.From(dictionary);

        metadata.Should().NotBeNull();
        metadata.HasMetadata().Should().Be(true);
        metadata.Count.Should().Be(2);
        metadata.GetMetadata("key1").Should().Be("value1");
        metadata.GetMetadata("key2").Should().Be(42);
    }

    [Fact]
    public void From_WithNullDictionary_ThrowsArgumentNullException()
    {
        // ParameterRequired checks the parameter NAME "dictionary", not the value
        // But the underlying Dictionary constructor throws when null is passed
        Action act = () => DefaultMetadata.From(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void From_WithEmptyDictionary_ThrowsArgumentException()
    {
        Action act = () => DefaultMetadata.From(new Dictionary<string, object>());
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.parameters.not_empty*");
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void From_WithKeyLength_ValidatesAtCreationTime(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var dictionary = new Dictionary<string, object> { [key] = "value" };

        if (shouldSucceed)
        {
            var metadata = DefaultMetadata.From(dictionary);
            metadata.Should().NotBeNull();
            metadata.Count.Should().Be(1);
            metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => DefaultMetadata.From(dictionary);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region Presence Tests

    [Fact]
    public void HasMetadata_EmptyMetadata_ReturnsFalse()
    {
        var metadata = DefaultMetadata.Empty();
        metadata.HasMetadata().Should().Be(false);
    }

    [Fact]
    public void HasMetadata_NonEmptyMetadata_ReturnsTrue()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        metadata.HasMetadata().Should().Be(true);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    [InlineData("key", "valid key")]
    public void HasMetadata_WithKey_ValidatesKey(string? key, string _)
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        if (string.IsNullOrWhiteSpace(key))
        {
            Action act = () => metadata.HasMetadata(key!);
            act.Should().Throw<ArgumentException>();
        }
        else
        {
            var result = metadata.HasMetadata(key!);
            result.Should().Be(key == "key");
        }
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void HasMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var metadata = DefaultMetadata.Empty();
        if (shouldSucceed) metadata.WithMetadata(key, "value");

        if (shouldSucceed)
        {
            metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => metadata.HasMetadata(key);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Fact]
    public void HasMetadata_WithOutParameter_ExistingKey_ReturnsTrueAndSetsValue()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        var result = metadata.HasMetadata("key", out var value);

        result.Should().Be(true);
        value.Should().Be("value");
    }

    [Fact]
    public void HasMetadata_WithOutParameter_NonExistingKey_ReturnsFalseAndSetsNull()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        var result = metadata.HasMetadata("nonexistent", out var value);

        result.Should().Be(false);
        value.Should().BeNull();
    }

    #endregion

    #region Retrieval Tests

    [Fact]
    public void GetMetadata_WithExistingKey_ReturnsValue()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        metadata.GetMetadata("key").Should().Be("value");
    }

    [Fact]
    public void GetMetadata_WithNonExistingKey_ReturnsNull()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        metadata.GetMetadata("nonexistent").Should().BeNull();
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void GetMetadata_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        var metadata = DefaultMetadata.Empty();

        Action act = () => metadata.GetMetadata(key!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void GetMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var metadata = DefaultMetadata.Empty();
        if (shouldSucceed) metadata.WithMetadata(key, "value");

        if (shouldSucceed)
        {
            metadata.GetMetadata(key).Should().Be("value");
        }
        else
        {
            Action act = () => metadata.GetMetadata(key);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Fact]
    public void GetMetadata_Generic_WithExistingKeyAndMatchingType_ReturnsTypedValue()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", 42));
        metadata.GetMetadata<int>("key").Should().Be(42);
    }

    [Fact]
    public void GetMetadata_Generic_WithExistingKeyAndNonMatchingType_ReturnsDefault()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "not an int"));
        metadata.GetMetadata<int>("key").Should().Be(default);
    }

    [Fact]
    public void GetMetadata_Generic_WithNonExistingKey_ReturnsDefault()
    {
        var metadata = DefaultMetadata.Empty();
        metadata.GetMetadata<int>("nonexistent").Should().Be(default);
    }

    [Fact]
    public void TryGetMetadata_WithExistingKey_ReturnsTrueAndSetsValue()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        var result = metadata.TryGetMetadata("key", out var value);

        result.Should().Be(true);
        value.Should().Be("value");
    }

    [Fact]
    public void TryGetMetadata_WithNonExistingKey_ReturnsFalseAndSetsNull()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        var result = metadata.TryGetMetadata("nonexistent", out var value);

        result.Should().Be(false);
        value.Should().BeNull();
    }

    [Fact]
    public void TryGetMetadata_Generic_WithExistingKeyAndMatchingType_ReturnsTrueAndSetsValue()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", 42));

        var result = metadata.TryGetMetadata("key", out int value);

        result.Should().Be(true);
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetMetadata_Generic_WithExistingKeyAndNonMatchingType_ReturnsFalseAndSetsDefault()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "not an int"));

        var result = metadata.TryGetMetadata("key", out int value);

        result.Should().Be(false);
        value.Should().Be(default);
    }

    [Fact]
    public void TryGetMetadata_Generic_WithNonExistingKey_ReturnsFalseAndSetsDefault()
    {
        var metadata = DefaultMetadata.Empty();

        var result = metadata.TryGetMetadata("nonexistent", out int value);

        result.Should().Be(false);
        value.Should().Be(default);
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void TryGetMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var metadata = DefaultMetadata.Empty();
        if (shouldSucceed) metadata.WithMetadata(key, "value");

        if (shouldSucceed)
        {
            metadata.TryGetMetadata(key, out var value).Should().Be(true);
            value.Should().Be("value");
        }
        else
        {
            Action act = () => metadata.TryGetMetadata(key, out object? _);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region Composition Tests

    [Fact]
    public void WithMetadata_SingleKeyValue_AddsOrUpdatesEntry()
    {
        var metadata = DefaultMetadata.Empty();
        var result = metadata.WithMetadata("key", "value");

        result.Should().BeSameAs(metadata);
        metadata.HasMetadata("key").Should().Be(true);
        metadata.GetMetadata("key").Should().Be("value");
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void WithMetadata_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        var metadata = DefaultMetadata.Empty();

        Action act = () => metadata.WithMetadata(key!, "value");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var metadata = DefaultMetadata.Empty();

        if (shouldSucceed)
        {
            var result = metadata.WithMetadata(key, "value");
            result.Should().BeSameAs(metadata);
            metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => metadata.WithMetadata(key, "value");
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Fact]
    public void WithMetadata_WithNullValue_StoresNull()
    {
        var metadata = DefaultMetadata.Empty();
        var result = metadata.WithMetadata("key", null!, isRequired: false);

        result.Should().BeSameAs(metadata);
        metadata.HasMetadata("key").Should().Be(true);
        metadata.GetMetadata("key").Should().BeNull();
    }

    [Fact]
    public void WithMetadata_UpdatesExistingKey()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "old"));
        metadata.WithMetadata("key", "new");
        metadata.GetMetadata("key").Should().Be("new");
    }

    [Fact]
    public void WithMetadata_MultipleParameters_AddsAllEntries()
    {
        var metadata = DefaultMetadata.Empty();
        var parameters = new[]
        {
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"),
        };

        var result = metadata.WithMetadata(parameters);

        result.Should().BeSameAs(metadata);
        metadata.Count.Should().Be(2);
        metadata.GetMetadata("key1").Should().Be("value1");
        metadata.GetMetadata("key2").Should().Be("value2");
    }

    [Fact]
    public void WithMetadata_WithNullParameters_ThrowsArgumentNullException()
    {
        var metadata = DefaultMetadata.Empty();

        // CollectionNotNull now validates the value
        Action act = () => metadata.WithMetadata((KeyValuePair<string, object>[])null!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*metadata.parameters.required*");
    }

    [Fact]
    public void WithMetadata_WithEmptyParameters_ThrowsArgumentException()
    {
        var metadata = DefaultMetadata.Empty();

        Action act = () => metadata.WithMetadata(Array.Empty<KeyValuePair<string, object>>());
        act.Should().Throw<ArgumentException>()
            .WithMessage("*metadata.parameters.not_empty*");
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithMetadata_WithParameterKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var parameters = new[] { new KeyValuePair<string, object>(key, "value") };
        var metadata = DefaultMetadata.Empty();

        if (shouldSucceed)
        {
            metadata.WithMetadata(parameters);
            metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => metadata.WithMetadata(parameters);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Fact]
    public void WithMetadata_IMetadata_MergesEntries()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key1", "value1"));
        var other = DefaultMetadata.Create(new KeyValuePair<string, object>("key2", "value2"));

        var result = metadata.WithMetadata(other);

        result.Should().BeSameAs(metadata);
        metadata.Count.Should().Be(2);
        metadata.GetMetadata("key1").Should().Be("value1");
        metadata.GetMetadata("key2").Should().Be("value2");
    }

    [Fact]
    public void WithMetadata_IMetadata_WithNull_ThrowsArgumentNullException()
    {
        var metadata = DefaultMetadata.Empty();

        // ParameterNotNull now validates the value
        Action act = () => metadata.WithMetadata((IMetadata)null!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*metadata.metadata.required*");
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithMetadata_IMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var metadata = DefaultMetadata.Empty();

        if (shouldSucceed)
        {
            var other = DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"));
            metadata.WithMetadata(other);
            metadata.HasMetadata(key).Should().Be(true);
        }
        else
        {
            Action act = () => DefaultMetadata.Create(new KeyValuePair<string, object>(key, "value"));
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    [Fact]
    public void WithMetadata_IMetadata_WithSelf_ReturnsSameInstance()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        var result = metadata.WithMetadata(metadata);

        result.Should().BeSameAs(metadata);
        metadata.Count.Should().Be(1);
    }

    #endregion

    #region Removal Tests

    [Fact]
    public void WithoutMetadata_NoParameters_ClearsAllEntries()
    {
        var metadata = DefaultMetadata.Create(
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"));

        var result = metadata.WithoutMetadata();

        result.Should().BeSameAs(metadata);
        metadata.HasMetadata().Should().Be(false);
        metadata.Count.Should().Be(0);
    }

    [Fact]
    public void WithoutMetadata_WithKey_RemovesEntry()
    {
        var metadata = DefaultMetadata.Create(
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"));

        var result = metadata.WithoutMetadata("key1");

        result.Should().BeSameAs(metadata);
        metadata.Count.Should().Be(1);
        metadata.HasMetadata("key1").Should().Be(false);
        metadata.HasMetadata("key2").Should().Be(true);
    }

    [Fact]
    public void WithoutMetadata_WithNonExistingKey_DoesNothing()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        var result = metadata.WithoutMetadata("nonexistent");

        result.Should().BeSameAs(metadata);
        metadata.Count.Should().Be(1);
        metadata.HasMetadata("key").Should().Be(true);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void WithoutMetadata_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        var metadata = DefaultMetadata.Empty();

        Action act = () => metadata.WithoutMetadata(key!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithoutMetadata_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var metadata = DefaultMetadata.Empty();
        if (shouldSucceed) metadata.WithMetadata(key, "value");

        if (shouldSucceed)
        {
            var result = metadata.WithoutMetadata(key);
            result.Should().BeSameAs(metadata);
            metadata.HasMetadata(key).Should().Be(false);
        }
        else
        {
            Action act = () => metadata.WithoutMetadata(key);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region Absence Tests

    [Fact]
    public void WithoutMetadata_OutParameter_WithExistingKey_ReturnsFalseAndRemovesEntry()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        var result = metadata.WithoutMetadata("key", out var value);

        result.Should().Be(false);
        value.Should().Be("value");
        metadata.HasMetadata("key").Should().Be(false);
    }

    [Fact]
    public void WithoutMetadata_OutParameter_WithNonExistingKey_ReturnsTrueAndSetsNull()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));

        var result = metadata.WithoutMetadata("nonexistent", out var value);

        result.Should().Be(true);
        value.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace")]
    public void WithoutMetadata_OutParameter_WithInvalidKey_ThrowsArgumentException(string? key, string _)
    {
        var metadata = DefaultMetadata.Empty();

        Action act = () => metadata.WithoutMetadata(key!, out object? _);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(255, true, "at max length")]
    [InlineData(256, false, "exceeding max length")]
    public void WithoutMetadata_OutParameter_WithKeyLength_ValidatesMaxLength(int keyLength, bool shouldSucceed, string _)
    {
        var key = new string('k', keyLength);
        var metadata = DefaultMetadata.Empty();
        if (shouldSucceed) metadata.WithMetadata(key, "value");

        if (shouldSucceed)
        {
            metadata.WithoutMetadata(key, out var value).Should().Be(false);
            value.Should().Be("value");
            metadata.HasMetadata(key).Should().Be(false);
        }
        else
        {
            Action act = () => metadata.WithoutMetadata(key, out object? _);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*metadata.key.max_length*");
        }
    }

    #endregion

    #region IDictionary Implementation Tests

    [Fact]
    public void IDictionary_ContainsKey_Works()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        metadata.ContainsKey("key").Should().Be(true);
        metadata.ContainsKey("nonexistent").Should().Be(false);
    }

    [Fact]
    public void IDictionary_TryGetValue_Works()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        metadata.TryGetValue("key", out var value).Should().Be(true);
        value.Should().Be("value");
        metadata.TryGetValue("nonexistent", out value).Should().Be(false);
        value.Should().BeNull();
    }

    [Fact]
    public void IDictionary_Indexer_GetAndSet_Works()
    {
        var metadata = DefaultMetadata.Empty();
        metadata["key"] = "value";
        metadata["key"].Should().Be("value");
    }

    [Fact]
    public void IDictionary_Add_Works()
    {
        var metadata = DefaultMetadata.Empty();
        metadata.Add("key", "value");
        metadata.Count.Should().Be(1);
        metadata.GetMetadata("key").Should().Be("value");
    }

    [Fact]
    public void IDictionary_Remove_Works()
    {
        var metadata = DefaultMetadata.Create(new KeyValuePair<string, object>("key", "value"));
        metadata.Remove("key").Should().Be(true);
        metadata.Count.Should().Be(0);
    }

    [Fact]
    public void IDictionary_Clear_Works()
    {
        var metadata = DefaultMetadata.Create(
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"));
        metadata.Clear();
        metadata.Count.Should().Be(0);
        metadata.HasMetadata().Should().Be(false);
    }

    [Fact]
    public void IDictionary_Keys_Works()
    {
        var metadata = DefaultMetadata.Create(
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"));
        var keys = metadata.Keys.ToList();
        keys.Should().HaveCount(2);
        keys.Should().Contain("key1");
        keys.Should().Contain("key2");
    }

    [Fact]
    public void IDictionary_Values_Works()
    {
        var metadata = DefaultMetadata.Create(
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"));
        var values = metadata.Values.ToList();
        values.Should().HaveCount(2);
        values.Should().Contain("value1");
        values.Should().Contain("value2");
    }

    [Fact]
    public void IDictionary_GetEnumerator_Works()
    {
        var metadata = DefaultMetadata.Create(
            new KeyValuePair<string, object>("key1", "value1"),
            new KeyValuePair<string, object>("key2", "value2"));
        var entries = metadata.ToList();
        entries.Should().HaveCount(2);
        entries.Should().Contain(e => e.Key == "key1" && e.Value.Equals("value1"));
        entries.Should().Contain(e => e.Key == "key2" && e.Value.Equals("value2"));
    }

    #endregion
}