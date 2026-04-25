using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Archi.Api.Contracts.Auth;
using Archi.Api.Contracts.Profile;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Archi.Api.Tests;

public sealed class ProfileEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProfileEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task PatchPrivacy_UpdatesIsPrivate()
    {
        var email = "privacy1@example.com";
        var register = new RegisterRequest(
            email,
            "privacyuser1",
            "testpass123");

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", register);
        registerResponse.EnsureSuccessStatusCode();
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(registerBody);
        var token = registerBody!.AccessToken;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(
            jwt.Claims,
            claim => claim.Type is JwtRegisteredClaimNames.Sub or "sub");

        using (var me = new HttpRequestMessage(HttpMethod.Get, "/api/v1/profile"))
        {
            me.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var meResponse = await _client.SendAsync(me);
            Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        }

        using (var unauthenticated = new HttpRequestMessage(
                   HttpMethod.Patch,
                   "/api/v1/profile/privacy")
               {
                   Content = JsonContent.Create(new { isPrivate = true })
               })
        {
            var unauth = await _client.SendAsync(unauthenticated);
            Assert.NotEqual(HttpStatusCode.NotFound, unauth.StatusCode);
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            "/api/v1/profile/privacy");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { isPrivate = true });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsPrivate);
    }

    [Fact]
    public async Task PublicUserProfile_Hidden_WhenPrivate_AndNotOwner()
    {
        var register = new RegisterRequest(
            "privateuser2@example.com",
            "privateuser2",
            "testpass123");

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", register);
        registerResponse.EnsureSuccessStatusCode();
        var body = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(body);
        var userId = body!.UserId;
        var token = body.AccessToken;

        using var lockRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            "/api/v1/profile/privacy");
        lockRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        lockRequest.Content = JsonContent.Create(new { isPrivate = true });
        (await _client.SendAsync(lockRequest)).EnsureSuccessStatusCode();

        var publicResponse = await _client.GetAsync($"/api/v1/users/{userId}/profile");
        Assert.Equal(HttpStatusCode.NotFound, publicResponse.StatusCode);
    }
}
