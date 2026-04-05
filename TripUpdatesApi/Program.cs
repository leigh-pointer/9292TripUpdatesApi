using TripUpdatesApi.Data;
using TripUpdatesApi.Services;

var builder = WebApplication.CreateBuilder(args);

// DI - Singleton for mock DB (simulates persistence)
builder.Services.AddSingleton<MockDatabase>();
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<UpdateLogService>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Trip Updates API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

public partial class Program { }
