using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TripUpdatesApi.Tests;

// Integration tests that exercise the full HTTP stack — routing, model binding,
// middleware, controllers, services, and the in-memory database — all in a
// single in-process test run.
//
// WebApplicationFactory<Program> boots the real ASP.NET Core application using
// the same Program.cs entry point as production, but replaces the Kestrel HTTP
// server with an in-memory TestServer.  This means:
//   - No network ports are opened.
//   - Tests run fast (no TCP overhead).
//   - The full middleware pipeline (Swagger, routing, model validation) is active.
//
// IClassFixture<WebApplicationFactory<Program>> tells xUnit to create one
// factory instance shared across all tests in this class, which avoids the
// overhead of booting the application for every single test.
//
// The partial class Program { } declaration in Program.cs is what makes
// WebApplicationFactory<Program> work — it exposes the entry-point type to
// this test assembly.
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // ── GET /trips ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTrips_ReturnsSeededData()
    {
        // Arrange: create an HttpClient that routes requests to the TestServer.
        var client = _factory.CreateClient();

        // Act: call the trips endpoint with no filters.
        var response = await client.GetAsync("/trips");

        // Assert: the response is 200 OK and the body contains at least one
        // trip (verified by the presence of the "tripId" JSON property).
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("tripId", content);
    }

    [Fact]
    public async Task GetTrips_WithLineFilter_FiltersCorrectly()
    {
        var client = _factory.CreateClient();

        // Act: filter to line 1 only.
        var response = await client.GetAsync("/trips?lineId=1");

        // Assert: the response must not contain any trips with lineNo 2,
        // confirming the filter was applied correctly end-to-end.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"lineNo\":2", content);
    }

    // ── POST /updates/trips ───────────────────────────────────────────────────

    [Fact]
    public async Task PostUpdates_ProcessesValidRequest()
    {
        var client = _factory.CreateClient();

        // Arrange: build a JSON payload for trip 1, 5 minutes late.
        // Using an anonymous object + JsonSerializer.Serialize avoids a hard
        // dependency on the DTO types in the test project.
        var payload = new
        {
            updates = new[]
            {
                new { tripId = 1, actualDeparture = "2026-03-27T08:05:00", actualArrival = "2026-03-27T09:05:00" }
            }
        };

        // Act: POST the update.
        var response = await client.PostAsync("/updates/trips",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        // Assert: 200 OK and the response body confirms 1 successful update.
        // Checking for "successCount":1 in the raw JSON is a lightweight way
        // to verify the business logic ran without deserialising the full DTO.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"successCount\":1", content);
    }

    // ── GET /updatelogs ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetUpdateLogs_ReturnsLogsAfterUpdate()
    {
        var client = _factory.CreateClient();

        // Arrange: first POST an update to ensure at least one log entry exists.
        // This test intentionally chains two HTTP calls to verify the full
        // write-then-read flow across the API boundary.
        var payload = new
        {
            updates = new[]
            {
                new { tripId = 2, actualDeparture = "2026-03-27T10:10:00", actualArrival = "2026-03-27T11:10:00" }
            }
        };

        await client.PostAsync("/updates/trips",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        // Act: query the logs endpoint.
        var response = await client.GetAsync("/updatelogs");

        // Assert: 200 OK and the body contains at least one log entry
        // (verified by the presence of the "updateLogId" JSON property).
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("updateLogId", content);
    }
}
