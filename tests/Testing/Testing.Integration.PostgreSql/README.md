# Testing.Integration.PostgreSql

PostgreSql container fixture for integration tests.

## Register

```csharp
services.AddPostgreSqlTestFixture(options => ...);
```

The fixture is registered as a singleton but only starts its container when
`StartAsync` is called. Use it once per test collection and reset state between tests
where the provider supports it.

## Options

The options type carries the container image and, for PostgreSQL, the database and
credentials. Invalid settings fail registration before a container is built.

## Known limits

- Requires a Docker or Podman endpoint. When Podman is installed but its API service is not
  running, `ContainerRuntime` starts a private service for the test process and stops it on exit;
  tests skip only when no endpoint can be established at all.
- Container startup dominates test time; share one fixture per collection.
