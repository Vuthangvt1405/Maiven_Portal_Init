# SQL Server connection setup

The API uses Entity Framework Core's SQL Server provider. Its connection string is read from the ASP.NET Core configuration key `ConnectionStrings:DefaultConnection`.

## Local development

1. Copy `.env.example` to `.env` in the project root.
2. Replace the placeholder server, database, username, and password with local SQL Server values.
3. Start the application with the `Development` launch profile:

   ```bash
   dotnet run --launch-profile https
   ```

The `.env` file is loaded only when `DOTNET_ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` is `Development`. Existing process environment variables take precedence over `.env` values.

The environment variable uses ASP.NET Core's double-underscore convention:

```text
ConnectionStrings__DefaultConnection
```

This maps to `ConnectionStrings:DefaultConnection`, which the application reads with `GetConnectionString("DefaultConnection")`.

## Connection string examples

SQL Server authentication:

```text
Server=localhost;Database=MaivenPortal;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True
```

Windows authentication:

```text
Server=localhost;Database=MaivenPortal;Integrated Security=True;Encrypt=True;TrustServerCertificate=True
```

`TrustServerCertificate=True` is intended only as a local-development convenience. Production should use a certificate that can be validated and should not enable this option without an explicit security review.

## Production

Do not deploy a `.env` file. Supply `ConnectionStrings__DefaultConnection` through the deployment platform's environment or secret manager. The application ignores `.env` outside Development and reports a sanitized startup error if the connection string is absent.

## Manual connectivity check

After configuring a reachable SQL Server, resolve `AppDbContext` from a development dependency-injection scope and evaluate:

```csharp
await dbContext.Database.CanConnectAsync();
```

A result of `true` confirms that EF Core can connect. Do not print or log the connection string when diagnosing failures. The application intentionally does not probe the database at startup and does not expose a database health endpoint.
