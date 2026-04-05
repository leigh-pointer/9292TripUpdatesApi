# Trip Updates API

A lightweight ASP.NET Core REST API for processing trip updates with validation and logging.

## Architecture

- **Controllers** - Thin API layer handling HTTP requests/responses
- **Services** - Business logic (TripService, UpdateLogService)
- **Data** - Mock database with seed data (in-memory persistence)
- **DTOs** - Request/response models for API contracts
- **Models** - Domain entities

## Choices & Trade-offs

| Choice | Rationale |
|--------|-----------|
| Mock DB (in-memory) | Fast iteration, no external dependencies for demo |
| Singleton DB | Simulates persistence within app lifecycle |
| ±2 min margin | Standard transit industry tolerance for on-time status |
| Swagger enabled | Self-documenting API, easy testing |

## What Would Differ in Production

- **Real database** (PostgreSQL/SQL Server) for persistence
- **EF Core** for ORM with migrations
- **Repository pattern** for data access abstraction
- **Validation middleware** (FluentValidation)
- **Logging** (Serilog) + structured logging
- **Authentication** (JWT) for API security
- **Health checks** endpoint
- **Async handlers** for scalability

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/trips?lineId=&from=&to=` | Query trips with filters |
| POST | `/updates/trips` | Process batch trip updates |
| GET | `/updatelogs?from=&to=&status=` | Query update logs |

## Run

```bash
cd TripUpdatesApi
dotnet run
```

Swagger UI: http://localhost:5000/swagger

## Test

```bash
dotnet test
```

## Example Requests

### Get Trips
```bash
curl http://localhost:5000/trips
curl http://localhost:5000/trips?lineId=1
```

### Process Updates
```bash
curl -X POST http://localhost:5000/updates/trips \
  -H "Content-Type: application/json" \
  -d '{
    "updates": [
      {"tripId": 1, "actualDeparture": "2026-03-27T08:05:00", "actualArrival": "2026-03-27T09:05:00"}
    ]
  }'
```

### Get Logs
```bash
curl http://localhost:5000/updatelogs
curl http://localhost:5000/updatelogs?status=Late
```
