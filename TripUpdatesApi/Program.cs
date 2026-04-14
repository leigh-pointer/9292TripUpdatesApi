using TripUpdatesApi.Data;
using TripUpdatesApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Persistence: Singleton in-memory store ────────────────────────────────────
// MockDatabase is registered as a Singleton so every request shares the same
// in-memory instance for the lifetime of the process.
// Trade-off: zero infrastructure overhead for this demo, at the cost of data
// being lost on restart.  In production this becomes a DbContext (EF Core)
// pointing at PostgreSQL or SQL Server.
builder.Services.AddSingleton<MockDatabase>();

// ── Domain services: one instance per request ─────────────────────────────────
// Controllers never touch the database directly — all domain logic lives in
// TripService (update processing) and UpdateLogService (audit log queries).
// Scoped lifetime means a clean service instance per HTTP request.
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<UpdateLogService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Trip Updates API", Version = "v1" });
});

var app = builder.Build();

// Swagger is unconditionally enabled — this is a demo, not a production deploy.
// In production it would be gated behind app.Environment.IsDevelopment().
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

// Exposes Program to the test project so WebApplicationFactory<Program> can
// boot the real application in-process for integration tests.
public partial class Program { }
