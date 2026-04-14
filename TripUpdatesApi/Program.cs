using TripUpdatesApi.Data;
using TripUpdatesApi.Services;

// ─────────────────────────────────────────────────────────────────────────────
// Entry point for the ASP.NET Core application.
// WebApplication.CreateBuilder sets up the host, configuration (appsettings.json,
// environment variables, command-line args) and the DI container in one call.
// ─────────────────────────────────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

// ── Dependency Injection registrations ───────────────────────────────────────
// MockDatabase is registered as a Singleton so the same in-memory instance is
// shared across every request for the lifetime of the process.  This simulates
// the persistence you would get from a real database without any external
// infrastructure.  In production this would be replaced by a DbContext
// registered with AddDbContext<T>().
builder.Services.AddSingleton<MockDatabase>();

// TripService and UpdateLogService are Scoped: a new instance is created per
// HTTP request.  This is the standard lifetime for services that depend on a
// database context, because it ties the service lifetime to the request
// boundary and avoids concurrency issues.
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<UpdateLogService>();

// ── MVC + OpenAPI ─────────────────────────────────────────────────────────────
// AddControllers registers the MVC pipeline for attribute-routed API controllers
// (no Razor views or pages needed for a pure REST API).
builder.Services.AddControllers();

// AddEndpointsApiExplorer makes minimal-API endpoints visible to Swagger.
// AddSwaggerGen generates the OpenAPI specification document at runtime.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Metadata shown in the Swagger UI header and in the generated spec file.
    c.SwaggerDoc("v1", new() { Title = "Trip Updates API", Version = "v1" });
});

// Build the WebApplication from the configured builder.
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
// UseSwagger serves the raw OpenAPI JSON at /swagger/v1/swagger.json.
// UseSwaggerUI serves the interactive browser UI at /swagger.
// Both are enabled unconditionally here because this is a demo/dev project;
// in production you would guard them with app.Environment.IsDevelopment().
app.UseSwagger();
app.UseSwaggerUI();

// MapControllers scans all classes decorated with [ApiController] and registers
// their action methods as HTTP endpoints based on their [Route] attributes.
app.MapControllers();

app.Run();

// ── Test-harness hook ─────────────────────────────────────────────────────────
// The partial class declaration makes Program visible to the integration-test
// project so WebApplicationFactory<Program> can boot the real application
// in-process during tests without any extra configuration.
public partial class Program { }
