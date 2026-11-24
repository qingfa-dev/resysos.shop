namespace BuildingBlock.Functional.Metadata;

public interface IMetadata : IDictionary<string, object>
{
    // Reference to constants for default values
    // ReSharper disable once InconsistentNaming
    public static class Defaults
    {
        public const bool ValueRequired = MetadataConstant.Defaults.ValueRequired;
        public const bool ValueNotNull = MetadataConstant.Defaults.ValueNotNull;
    }
   #region Factory
    static abstract IMetadata Empty();

    static abstract IMetadata Create(
        params KeyValuePair<string, object>[] parameters);

    static abstract IMetadata From(
        IDictionary<string, object> dictionary);

    #endregion
    #region Presence

    bool HasMetadata();

    bool HasMetadata(string key);

    bool HasMetadata(
        string key,
        out object? value);

    #endregion
    #region Retrieval

    object? GetMetadata(string key);

    T? GetMetadata<T>(string key);

    bool TryGetMetadata(
        string key,
        out object? value);

    bool TryGetMetadata<T>(
        string key,
        out T? value);

    #endregion
    #region Composition

    IMetadata WithMetadata(string key, object value, bool isRequired = Defaults.ValueNotNull);

    IMetadata WithMetadata(
        params KeyValuePair<string, object>[] parameters);

    IMetadata WithMetadata(IMetadata metadata);

    #endregion
    #region Removal

    IMetadata WithoutMetadata();

    IMetadata WithoutMetadata(string key);

    #endregion
    #region Absence

    bool WithoutMetadata(string key, out object? value);
    #endregion
}