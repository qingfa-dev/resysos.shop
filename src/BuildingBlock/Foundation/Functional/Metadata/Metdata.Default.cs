namespace BuildingBlock.Functional.Metadata;

public class DefaultMetadata : Dictionary<string, object>, IMetadata
{
    #region Constructors
    private DefaultMetadata()
    {
    }

    private DefaultMetadata(
        IEnumerable<KeyValuePair<string, object>> parameters)
        : base(ValidateAndFilterParameters(parameters))
    {
    }

    private static IEnumerable<KeyValuePair<string, object>> ValidateAndFilterParameters(
        IEnumerable<KeyValuePair<string, object>> parameters)
    {
        MetadataGuard.CollectionRequired(parameters, nameof(parameters));
        MetadataGuard.CollectionNotEmpty(parameters, nameof(parameters));

        foreach (var parameter in parameters)
        {
            MetadataGuard.KeyValid(parameter.Key);
            MetadataGuard.ValueNotNull(parameter.Value);
        }

        return parameters;
    }
    #endregion

    #region Factory
    public static IMetadata Empty()
        => new DefaultMetadata();

    public static IMetadata Create(
        params KeyValuePair<string, object>[] parameters)
    {
        MetadataGuard.CollectionRequired(parameters, nameof(parameters));
        MetadataGuard.CollectionNotEmpty(parameters, nameof(parameters));

        foreach (var parameter in parameters)
        {
            MetadataGuard.KeyValid(parameter.Key);
            MetadataGuard.ValueNotNull(parameter.Value);
        }

        return new DefaultMetadata(parameters);
    }

    public static IMetadata From(
        IDictionary<string, object> dictionary)
    {
        MetadataGuard.ParameterNotNull(dictionary, nameof(dictionary));

        foreach (var kvp in dictionary)
        {
            MetadataGuard.KeyValid(kvp.Key);
            MetadataGuard.ValueNotNull(kvp.Value);
        }

        return new DefaultMetadata(dictionary);
    }
    #endregion

    #region Presence

    public bool HasMetadata()
        => Count > 0;

    public bool HasMetadata(string key)
    {
        MetadataGuard.KeyValid(key);
        return ContainsKey(key);
    }

    public bool HasMetadata(
        string key,
        out object? value)
        => TryGetMetadata(key, out value);

    #endregion

    #region Retrieval

    public object? GetMetadata(string key)
    {
        MetadataGuard.KeyValid(key);
        return TryGetValue(key, out var value)
            ? value
            : null;
    }

    public T? GetMetadata<T>(string key)
    {
        MetadataGuard.KeyValid(key);
        return TryGetValue(key, out var value) && value is T result
            ? result
            : default;
    }

    public bool TryGetMetadata(
        string key,
        out object? value)
    {
        MetadataGuard.KeyValid(key);
        return TryGetValue(key, out value);
    }

    public bool TryGetMetadata<T>(
        string key,
        out T? value)
    {
        MetadataGuard.KeyValid(key);

        if (TryGetValue(key, out var result) && result is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    #endregion

    #region Composition

    public IMetadata WithMetadata(
        string key,
        object value,
        bool isRequired = MetadataConstant.Defaults.ValueNotNull)
    {
        MetadataGuard.KeyValid(key);
        MetadataGuard.ValueNotNull(value, isRequired);
        this[key] = value;

        return this;
    }

    public IMetadata WithMetadata(
        params KeyValuePair<string, object>[] parameters)
    {
        MetadataGuard.CollectionRequired(parameters, nameof(parameters));
        MetadataGuard.CollectionNotEmpty(parameters, nameof(parameters));

        foreach (var parameter in parameters)
        {
            MetadataGuard.KeyValid(parameter.Key);
            MetadataGuard.ValueNotNull(parameter.Value);
            WithMetadata(parameter.Key, parameter.Value);
        }

        return this;
    }

    public IMetadata WithMetadata(IMetadata metadata)
    {
        MetadataGuard.ParameterNotNull(metadata, nameof(metadata));

        if (ReferenceEquals(this, metadata))
            return this;

        foreach (var parameter in metadata)
        {
            MetadataGuard.KeyValid(parameter.Key);
            MetadataGuard.ValueNotNull(parameter.Value);
            WithMetadata(parameter.Key, parameter.Value);
        }

        return this;
    }

    #endregion

    #region Removal
    public IMetadata WithoutMetadata()
    {
        Clear();

        return this;
    }

    public IMetadata WithoutMetadata(string key)
    {
        MetadataGuard.KeyValid(key);
        Remove(key);

        return this;
    }

    #endregion

    #region Absence

    public bool WithoutMetadata(
        string key,
        out object? value)
    {
        MetadataGuard.KeyValid(key);
        if (!TryGetValue(key, out value))
            return true;

        Remove(key);

        return false;
    }

    #endregion

    #region IDictionary Implementation Notes

    // Note: DefaultMetadata inherits from Dictionary<string, object> for convenience.
    // The base Dictionary methods (Add, Remove, indexer, etc.) do NOT perform validation.
    // For validated operations, use the WithMetadata, WithoutMetadata, and factory methods.
    // The IDictionary interface is implemented to allow interoperability with existing code.

    #endregion
}