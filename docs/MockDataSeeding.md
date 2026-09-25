# Development mock-data seeding

The repository includes an explicitly invoked C# seeder that applies pending Entity Framework Core migrations and inserts a deterministic, connected development dataset into SQL Server.

## Safety

The seeder runs only when `DOTNET_ENVIRONMENT` or `ASPNETCORE_ENVIRONMENT` is `Development`. It does not run during API startup and must not be used as a production bootstrap mechanism.

The seeder reads `ConnectionStrings__DefaultConnection` through the same `.env` convention as the API. It never prints the connection string.

## Run

From the repository root:

```bash
DOTNET_ENVIRONMENT=Development dotnet run --project tools/MockDataSeeder/MockDataSeeder.csproj
```

On PowerShell:

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project tools/MockDataSeeder/MockDataSeeder.csproj
```

If `.env` already defines `DOTNET_ENVIRONMENT=Development` or `ASPNETCORE_ENVIRONMENT=Development`, setting it in the shell is unnecessary.

## Generated accounts

The migration-owned administrator is reused but not modified:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@example.com` | Migration-defined temporary password |
| Teacher | `mock.teacher01@maiven.local` through `mock.teacher03@maiven.local` | `MockPassword123!` |
| Student | `mock.student01@maiven.local` through `mock.student15@maiven.local` | `MockPassword123!` |

Students and Teachers authenticate through `POST /api/auth/login`. The administrator authenticates through `POST /api/auth/admin/login`.

The shared password is development-only and must never be used for production accounts.

Mock grade-component scores use a 0-10 scale. Course results use the same scale,
with 6 as the passing score and grade bands A (9+), B (8+), C (7+), D (6+), and F (<6).

## Generated dataset

A successful first run creates:

- 3 Teachers and 15 Students, each with one active role assignment
- 1 completed academic year and 2 semesters
- 5 courses and 10 course sections
- 2 registration periods
- 11 announcements
- 45 enrollments
- 30 grade components
- 135 student scores
- 45 course results

The academic year is deliberately `COMPLETED` so seeding cannot conflict with the database constraint allowing only one active academic year.

## Repeat runs

The seeder is idempotent. It identifies its records using reserved mock emails, names, titles, course codes, and compound keys. A repeat run:

- updates mock records to their canonical values;
- resets mock Student and Teacher passwords to `MockPassword123!`;
- reactivates soft-deleted mock records;
- does not duplicate mock records; and
- does not delete or modify unrelated records.

If a reserved mock identity has incompatible ownership, such as a mock Teacher email assigned to the Student role, the operation fails and its transaction is rolled back.

There is intentionally no destructive cleanup command. Remove mock records only in a disposable development database or through a separately reviewed cleanup process.
