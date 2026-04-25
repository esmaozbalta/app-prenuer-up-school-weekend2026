using System.Net;
using System.Net.Http.Json;
using Archi.Api.Contracts.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Archi.Api.Tests;

public sealed class RegisterEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Register_ReturnsCreated_AndAccessToken()
    {
        var request = new RegisterRequest(
            "test@example.com",
            "testuser",
            "testpass123");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var payload = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
        Assert.Equal("testuser", payload.Username);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenUsernameExists()
    {
        var firstRequest = new RegisterRequest(
            "first@example.com",
            "duplicateuser",
            "testpass123");
        var secondRequest = new RegisterRequest(
            "second@example.com",
            "duplicateuser",
            "testpass123");

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", firstRequest);
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", secondRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithAccessToken_WhenCredentialsValid()
    {
        var registerRequest = new RegisterRequest(
            "loginuser@example.com",
            "loginuser",
            "testpass123");

        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            "loginuser@example.com",
            "testpass123");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
        Assert.Equal("loginuser", payload.Username);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordInvalid()
    {
        var registerRequest = new RegisterRequest(
            "wrongpass@example.com",
            "wrongpassuser",
            "testpass123");

        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest(
            "wrongpass@example.com",
            "invalidpass");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
