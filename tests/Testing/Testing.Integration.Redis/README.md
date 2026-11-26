# Testing.Integration.Redis

Redis container fixture for integration tests.

## Register

```csharp
services.AddRedisTestFixture(options => ...);
```

The fixture is registered as a singleton but only starts its container when
`StartAsync` is called. Use it once per test collection and reset state between tests
where the provider supports it.

## Options

The options type carries the container image and, for PostgreSQL, the database and
credentials. Invalid settings fail registration before a container is built.

## Known limits

- Requires a Docker or Podman endpoint; tests skip when `ContainerRuntime.IsAvailable()`
  is false.
- Container startup dominates test time; share one fixture per collection.
