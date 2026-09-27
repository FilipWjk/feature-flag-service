using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FeatureFlags.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FeatureFlags.Api.Tests;

public class FeatureEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task FullRollout_ReturnsEnabled()
    {
        var body = await _client.GetFromJsonAsync<FeatureEnabledResponse>(
            "/api/features/DarkMode/enabled?userId=123");

        Assert.NotNull(body);
        Assert.True(body.Enabled);
    }

    [Fact]
    public async Task DisabledFeature_ReturnsDisabled()
    {
        var body = await _client.GetFromJsonAsync<FeatureEnabledResponse>(
            "/api/features/LegacyExport/enabled?userId=123");

        Assert.NotNull(body);
        Assert.False(body.Enabled);
    }

    [Fact]
    public async Task BlankUserId_ReturnsValidationProblem()
    {
        var response = await _client.GetAsync("/api/features/DarkMode/enabled?userId=%20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.GetProperty("errors").TryGetProperty("userId", out _));
    }

    [Fact]
    public async Task UnknownFeature_ReturnsProblemDetails()
    {
        var response = await _client.GetAsync("/api/features/Unknown/enabled?userId=123");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/openapi/v1.yaml", "application/yaml")]
    [InlineData("/swagger/index.html", "text/html")]
    public async Task Development_Documentation_IsServed(string path, string contentType)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
    }
}