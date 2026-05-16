using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Archi.Api.Contracts.Auth;
using Archi.Api.Contracts.Users;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Archi.Api.Tests;

public sealed class UserCrudEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserCrudEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task PostUsers_CreatesUser_SameAsRegister()
    {
        var request = new RegisterRequest(
            "crud-create@example.com",
            "crudcreateuser",
            "testpass123");

        var response = await _client.PostAsJsonAsync("/api/v1/users", request);
        var payload = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
    }

    [Fact]
    public async Task GetUser_ReturnsPublicProfile()
    {
        var register = new RegisterRequest(
            "crud-get@example.com",
            "crudgetuser",
            "testpass123");

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users", register);
        registerResponse.EnsureSuccessStatusCode();
        var body = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(body);
        var userId = body!.UserId;

        var getResponse = await _client.GetAsync($"/api/v1/users/{userId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var user = await getResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal(userId, user!.Id);
        Assert.Equal("crudgetuser", user.Username);
        Assert.Null(user.OauthId);
    }

    [Fact]
    public async Task GetUsers_ReturnsOnlyPublicProfiles()
    {
        var reg = new RegisterRequest(
            "crud-list@example.com",
            "crudlistuser",
            "testpass123");
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users", reg);
        registerResponse.EnsureSuccessStatusCode();
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(registered);

        var listResponse = await _client.GetAsync("/api/v1/users?page=1&pageSize=10");
        listResponse.EnsureSuccessStatusCode();
        var list = await listResponse.Content.ReadFromJsonAsync<UserListResponse>();
        Assert.NotNull(list);
        Assert.NotEmpty(list!.Items);
        Assert.Contains(list.Items, item => item.Id == registered!.UserId);
    }

    [Fact]
    public async Task PutUser_UpdatesUsername_WhenAuthorized()
    {
        var register = new RegisterRequest(
            "crud-put@example.com",
            "crudputuser",
            "testpass123");

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users", register);
        registerResponse.EnsureSuccessStatusCode();
        var body = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(body);
        var token = body!.AccessToken;
        var userId = body.UserId;

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new UpdateUserRequest("newcrudname", null, null, null));

        var putResponse = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        var updated = await putResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(updated);
        Assert.Equal("newcrudname", updated!.Username);
    }

    [Fact]
    public async Task DeleteUser_RemovesUser_WhenPasswordValid()
    {
        var register = new RegisterRequest(
            "crud-del@example.com",
            "cruddeluser",
            "testpass123");

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users", register);
        registerResponse.EnsureSuccessStatusCode();
        var body = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(body);
        var token = body!.AccessToken;
        var userId = body.UserId;

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/users/{userId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        deleteRequest.Content = JsonContent.Create(new DeleteUserRequest("testpass123"));

        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfter = await _client.GetAsync($"/api/v1/users/{userId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }
}
