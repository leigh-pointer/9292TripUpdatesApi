using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TripUpdatesApi.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTrips_ReturnsSeededData()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/trips");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("tripId", content);
    }

    [Fact]
    public async Task GetTrips_WithLineFilter_FiltersCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/trips?lineId=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"lineNo\":2", content);
    }

    [Fact]
    public async Task PostUpdates_ProcessesValidRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var payload = new
        {
            updates = new[]
            {
                new { tripId = 1, actualDeparture = "2026-03-27T08:05:00", actualArrival = "2026-03-27T09:05:00" }
            }
        };

        // Act
        var response = await client.PostAsync("/updates/trips",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"successCount\":1", content);
    }

    [Fact]
    public async Task GetUpdateLogs_ReturnsLogsAfterUpdate()
    {
        // Arrange
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

        // Act
        var response = await client.GetAsync("/updatelogs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("updateLogId", content);
    }
}
