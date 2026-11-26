namespace Testing.Data;

/// <summary>Small, deterministic-ish data generators for tests.</summary>
public static class TestData
{
    private static readonly string[] Words =
    [
        "alpha", "bravo", "charlie", "delta", "echo", "foxtrot"
    ];

    /// <summary>Gets a random word.</summary>
    public static string Word() => Words[Random.Shared.Next(Words.Length)];

    /// <summary>Gets a random identifier.</summary>
    public static Guid Id() => Guid.NewGuid();

    /// <summary>Gets a random email address.</summary>
    public static string Email() => $"{Word()}.{Random.Shared.Next(1000, 9999)}@example.test";
}
