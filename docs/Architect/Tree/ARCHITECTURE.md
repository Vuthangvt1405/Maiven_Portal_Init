# Application Architecture

The application is a C# layered monolith with three explicit runtime boundaries:

```text
API -> Service -> Repository -> SQL Server
```

This repository currently contains architecture scaffolding only. Application classes, project files, configuration, database scripts, and tests will be added during implementation.

## Structure

```text
Architect.sln
├── src/
│   ├── Architect.Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   │   └── GlobalExceptionHandling/
│   │   ├── Configuration/
│   │   └── Properties/
│   │
│   ├── Architect.Service/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   └── Dtos/
│   │       ├── Requests/
│   │       └── Responses/
│   │
│   ├── Architect.Repository/
│   │   ├── Interfaces/
│   │   ├── Repositories/
│   │   ├── Data/
│   │   └── Entities/
│   │
│   └── database/
│
└── tests/
```

Empty leaf directories contain `.gitkeep` files so the architecture can be versioned before implementation.

## Layer responsibilities

### Architect.Api

- Exposes HTTP controllers and standard API routes.
- Configures authentication and authorization.
- Hosts Swagger/OpenAPI configuration.
- Runs request logging and other API middleware.
- Handles unhandled exceptions globally and returns consistent HTTP error responses.
- Must call the Service layer instead of accessing repositories directly.

### Architect.Service

- Defines service interfaces and implementations.
- Owns application and business rules.
- Coordinates repository operations.
- Defines request and response DTOs.
- Must not depend on the API layer.

A dedicated validator directory is intentionally omitted. FluentValidation remains a delivery requirement and can be introduced beside request DTOs when API implementation begins.

### Architect.Repository

- Defines repository interfaces and implementations.
- Owns SQL Server persistence access.
- Contains persistence entities and database connection infrastructure.
- Uses parameterized database operations to protect against SQL injection.
- Must not depend on the API or Service implementations.

### Database

`src/database/` will contain the complete SQL Server initialization script, including tables, relationships, constraints, indexes, defaults, and required seed data. The script remains authoritative and independent from application startup.

### Tests

`tests/` will contain automated checks for layer behavior, API errors, validation, authorization, and persistence security when implementation begins.

## Dependency rules

```text
Architect.Api
    |
    v
Architect.Service
    |
    v
Architect.Repository
    |
    v
SQL Server
```

1. Dependencies move in one direction only.
2. Controllers never call repositories directly.
3. Repository code contains no HTTP or business workflow logic.
4. Entities are not returned directly by API controllers; DTOs define API contracts.
5. Global exception handling and request logging belong to API middleware.
6. Operational log4net records and business audit records remain separate concerns.
7. Role-based behavior is implemented through authorization—not separate Student, Teacher, or Admin application layers.

## Scope boundary

This scaffold intentionally does not yet include:

- C# implementation classes
- `.csproj` project definitions
- NuGet packages
- application settings or secrets
- log4net configuration
- SQL initialization statements
- Swagger configuration
- FluentValidation rules
- authentication implementation
- executable tests

Those items belong to the implementation phase, not the architecture-only scaffold.]

 The essential flow is:                                                                                                                                                    
                                                                                                                                                                           
 ```text                                                                                                                                                                   
   HTTP Request                                                                                                                                                            
       ↓                                                                                                                                                                   
   Global Exception Middleware                                                                                                                                             
       ↓                                                                                                                                                                   
   Request Logging Middleware                                                                                                                                              
       ↓                                                                                                                                                                   
   Routing                                                                                                                                                                 
       ↓                                                                                                                                                                   
   Authentication                                                                                                                                                          
       ↓                                                                                                                                                                   
   Authorization                                                                                                                                                           
       ↓                                                                                                                                                                   
   Model Binding and Validation                                                                                                                                            
       ↓                                                                                                                                                                   
   Controller                                                                                                                                                              
       ↓                                                                                                                                                                   
   Service                                                                                                                                                                 
       ↓                                                                                                                                                                   
   Repository                                                                                                                                                              
       ↓                                                                                                                                                                   
   SQL Server                                                                                                                                                              
       ↓                                                                                                                                                                   
   Repository result                                                                                                                                                       
       ↓                                                                                                                                                                   
   Service response DTO                                                                                                                                                    
       ↓                                                                                                                                                                   
   Controller HTTP result                                                                                                                                                  
       ↓                                                                                                                                                                   
   JSON serialization                                                                                                                                                      
       ↓                                                                                                                                                                   
   Logging completion                                                                                                                                                      
       ↓                                                                                                                                                                   
   HTTP Response                                                                                                                                                           
 ```  