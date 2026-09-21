using DotNetEnv;
using Maiven_Portal_Managment.Data;
using Microsoft.EntityFrameworkCore;
using MockDataSeederTool;

Env.NoClobber().TraversePath().Load();

var environmentName =
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (!string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine(
        "Mock data seeding is restricted to the Development environment. " +
        "Set DOTNET_ENVIRONMENT or ASPNETCORE_ENVIRONMENT to Development.");
    return 1;
}

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine(
        "ConnectionStrings__DefaultConnection is not configured. " +
        "Configure it in the environment or the development .env file.");
    return 1;
}

using var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(
        connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure())
    .Options;

try
{
    Console.WriteLine("Applying pending database migrations...");
    await using (var migrationContext = new AppDbContext(options))
    {
        await migrationContext.Database.MigrateAsync(cancellationSource.Token);
    }

    MockSeedSummary? summary = null;
    await using (var strategyContext = new AppDbContext(options))
    {
        var strategy = strategyContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var dbContext = new AppDbContext(options);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                cancellationSource.Token);

            var seeder = new MockDataSeeder(dbContext);
            summary = await seeder.SeedAsync(cancellationSource.Token);

            await transaction.CommitAsync(cancellationSource.Token);
        });
    }

    Console.WriteLine("Mock data seeding completed successfully.");
    Console.WriteLine(summary);
    Console.WriteLine("Mock Student and Teacher password: MockPassword123!");
    return 0;
}
catch (OperationCanceledException) when (cancellationSource.IsCancellationRequested)
{
    Console.Error.WriteLine("Mock data seeding was cancelled.");
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"Mock data seeding failed ({exception.GetType().Name}). " +
        "Check the development database configuration and application logs.");
    return 1;
}
