using System.Net;
using System.Net.Http.Json;
using Synapse.Backend.DTOs;
using Xunit;

namespace Synapse.Tests;

public class AuthenticationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_WithoutJwt_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Users");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Register_WithValidUser_ReturnsToken()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var request = new RegisterRequest
        {
            Username = $"test_{uniqueId}",
            Email = $"{uniqueId}@test.dk",
            Password = "Test1234!"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            request
        );

        Assert.True(response.IsSuccessStatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.UserId > 0);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var username = $"login_{uniqueId}";
        var password = "Test1234!";

        var registerRequest = new RegisterRequest
        {
            Username = username,
            Email = $"{uniqueId}@test.dk",
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            registerRequest
        );

        Assert.True(registerResponse.IsSuccessStatusCode);

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            loginRequest
        );

        Assert.True(loginResponse.IsSuccessStatusCode);

        var result = await loginResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.UserId > 0);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var username = $"wrongpass_{uniqueId}";

        var registerRequest = new RegisterRequest
        {
            Username = username,
            Email = $"{uniqueId}@test.dk",
            Password = "Correct123!"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            registerRequest
        );

        Assert.True(registerResponse.IsSuccessStatusCode);

        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = "WrongPassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            loginRequest
        );

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            loginResponse.StatusCode
        );
    }
}