using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TripUpdatesApi.Tests;

// Integration tests that exercise the full HTTP stack end-to-end:
// routing → model binding → controller → service → in-memory database → response.
//
// WebApplicationFactory<Program> boots the real application using the same
// Program.cs as production but with an in-process TestServer instead of Kestrel
// — no ports opened, no network overhead, full middleware pipeline active.
//
// One factory instance is shared across all tests in this class (IClassFixture)
// so the application only boots once, and the Singleton MockDatabase is shared
// between tests — matching how the real app behaves across requests.
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
        // Proves the seed data is reachable through the full HTTP stack.
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/trips");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("tripId", content);
    }

    [Fact]
    public async Task GetTrips_WithLineFilter_FiltersCorrectly()
    {
        // Proves the lineId filter works end-to-end: no line-2 trips in the response.
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/trips?lineId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"lineNo\":2", content);
    }

    // ── POST /updates/trips ───────────────────────────────────────────────────

    [Fact]
    public async Task PostUpdates_ProcessesValidRequest()
    {
        // Proves the update pipeline runs successfully through HTTP:
        // JSON body → model binding → TripService → BatchUpdateResult with successCount = 1.
        var client = _factory.CreateClient();
        var payload = new
        {
            updates = new[]
            {
                new { tripId = 1, actualDeparture = "2026-03-27T08:05:00", actualArrival = "2026-03-27T09:05:00" }
            }
        };

        var response = await client.PostAsync("/updates/trips",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"successCount\":1", content);
    }

    // ── GET /updatelogs ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetUpdateLogs_ReturnsLogsAfterUpdate()
    {
        // Proves the write-then-read flow: POST an update, then confirm the
        // audit log is queryable through GET /updatelogs.
        var client = _factory.CreateClient();
        var payload = new
        {
            updates = new[]
            {
                new { tripId = 2, actualDeparture = "2026-03-27T10:10:00", actualArrival = "2026-03-27T11:10:00" }
            }
        };

        await client.PostAsync("/updates/trips",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        var response = await client.GetAsync("/updatelogs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("updateLogId", content);
    }
}
