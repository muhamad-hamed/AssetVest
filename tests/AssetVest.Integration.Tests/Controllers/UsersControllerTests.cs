using AssetVest.Application.DTOs.Users;
using AssetVest.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AssetVest.Integration.Tests.Controllers;

public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            FirstName = "Integration",
            LastName = "Test",
            Email = "integration.test@example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeCurrentUserPassword_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword1!",
            NewPassword = "NewPassword1!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users/me/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListAndLookupRoutes_AreNotExposed()
    {
        // Enumeration routes were removed; no admin role exists to gate them.
        var byEmail = await _client.GetAsync("/api/v1/users/by-email/someone@example.com");

        byEmail.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
