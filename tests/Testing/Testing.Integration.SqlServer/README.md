# Testing.Integration.SqlServer

SQL Server container fixture for integration tests.

## Register

```csharp
services.AddSqlServerTestFixture(options => ...);
```

The fixture is registered as a singleton but only starts its container when
`StartAsync` is called. Use it once per test collection and reset state between tests
with `ResetAsync(params string[] schemas)`.

## Options

The options type carries the container image, the database name, and the `sa` password.
Invalid settings fail registration before a container is built.

## Known limits

- Requires a Docker or Podman endpoint. When Podman is installed but its API service is not
  running, `ContainerRuntime` starts a private service for the test process and stops it on exit;
  tests skip only when no endpoint can be established at all.
- The container is built with `ContainerBuilder` instead of `MsSqlBuilder`, because that
  builder probes readiness and creates the database with `sqlcmd` exec sessions. Rootless
  Podman denies the `oom_score_adj` write crun performs for exec sessions. This fixture
  waits on the server log, confirms the connection from the host, and creates the database
  over TDS instead.
- Container startup dominates test time; share one fixture per collection.
