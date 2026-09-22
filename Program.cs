using System.Reflection;
using System.Text.Json.Serialization;
using DotNetEnv;
using log4net;
using log4net.Config;
using Maiven_Portal_Managment.Configuration;
using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Middleware;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var environmentName =
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
    Environments.Production;

// Load local secrets from .env only during development. Existing environment
// variables take precedence so deployment-provided secrets are never replaced.
if (environmentName == Environments.Development)
{
    Env.NoClobber().TraversePath().Load();
}

var builder = WebApplication.CreateBuilder(args);

var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
Directory.CreateDirectory(logDirectory);
GlobalContext.Properties["LogFilePath"] = Path.Combine(logDirectory, "api.log");

var logRepository = LogManager.GetRepository(
    Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
var logConfigurationPath = Path.Combine(AppContext.BaseDirectory, "log4net.config");

if (!File.Exists(logConfigurationPath))
{
    throw new InvalidOperationException(
        $"The log4net configuration file was not found at '{logConfigurationPath}'.");
}

XmlConfigurator.Configure(logRepository, new FileInfo(logConfigurationPath));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The ConnectionStrings__DefaultConnection environment variable is not configured.");
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AcademicYearRepository>();
builder.Services.AddScoped<SemesterRepository>();
builder.Services.AddScoped<CourseSectionRepository>();
builder.Services.AddScoped<EnrollmentRepository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<AnnouncementRepository>();
builder.Services.AddScoped<CourseResultRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<SemesterService>();
builder.Services.AddScoped<AcademicYearService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<CourseSectionService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<CourseResultService>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter a JWT access token."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var app = builder.Build();

// Request logging is outermost so one line is written per request (try/finally),
// even when the exception middleware below handles a failure.
app.UseMiddleware<RequestLoggingMiddleware>();

// Keep exception handling inside the request logger so it can catch failures
// from all downstream middleware.
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
