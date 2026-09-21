# Maiven Portal Management API

ASP.NET Core API organized as `Controller -> Service -> Repository -> SQL Server`.

Controllers and URLs are resource-oriented. Roles remain authorization rules rather than controller or route prefixes. Each user has one active role assignment; authentication responses and JWTs expose its singular `role` and `roleUserId` (`USER_ROLES.id`). See [the resource route map](docs/ResourceControllerRoutes.md), [authentication workflow](docs/AuthWorkflow.md), and [academic-year workflow](docs/AcademicYearWorkflow.md).
