using System;
using System.Linq;
using Accounts.Core.Entities;
using Accounts.Infrastructure.Persistence;
using Xunit;

public class SessionStoreTests
{
    private readonly SessionStore _sessionStore;

    public SessionStoreTests()
    {
        _sessionStore = new SessionStore();
    }

    [Fact]
    public void AddSession_ShouldAddSessionSuccessfully()
    {
        // Arrange
        var token = "test-token";
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddMinutes(30); // Valid expiry time

        // Act
        _sessionStore.AddSession(token, userId, expiry);

        // Assert
        var session = _sessionStore.GetAllSessions().FirstOrDefault(s => s.UserId == userId);
        Assert.NotNull(session); // Session should be added
        Assert.Equal(userId, session?.UserId);
        Assert.Equal(expiry, session?.Expiry);
    }

    [Fact]
    public void TryGetSession_ShouldReturnValidSession()
    {
        // Arrange
        var token = "test-token";
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddMinutes(30); // Valid expiry time
        _sessionStore.AddSession(token, userId, expiry);

        // Act
        var result = _sessionStore.TryGetSession(token, out Session? session);

        // Assert
        Assert.True(result); // Session should be valid
        Assert.NotNull(session); // Session should be returned
        Assert.Equal(userId, session?.UserId); // Session userId should match
        Assert.Equal(expiry, session?.Expiry); // Expiry should match
    }

    [Fact]
    public void TryGetSession_ShouldReturnFalseForExpiredSession()
    {
        // Arrange
        var token = "test-token";
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddMinutes(-1); // Expired expiry time
        _sessionStore.AddSession(token, userId, expiry);

        // Act
        var result = _sessionStore.TryGetSession(token, out Session? session);

        // Assert
        Assert.False(result); // Session should be expired
        Assert.Null(session); // Session should be removed automatically
    }

    [Fact]
    public void RemoveSession_ShouldRemoveSessionSuccessfully()
    {
        // Arrange
        var token = "test-token";
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddMinutes(30); // Valid expiry time
        _sessionStore.AddSession(token, userId, expiry);

        // Act
        var result = _sessionStore.RemoveSession(token);

        // Assert
        Assert.True(result); // Session should be removed
        var session = _sessionStore.GetAllSessions().FirstOrDefault(s => s.UserId == userId);
        Assert.Null(session); // No session should be found
    }

    [Fact]
    public void RemoveSession_ShouldReturnFalseIfTokenIsEmpty()
    {
        // Act
        var result = _sessionStore.RemoveSession("");

        // Assert
        Assert.False(result); // It should return false because the token is invalid
    }

    [Fact]
    public void GetAllSessions_ShouldReturnAllSessions()
    {
        // Arrange
        var token1 = "token1";
        var userId1 = Guid.NewGuid();
        var expiry1 = DateTime.UtcNow.AddMinutes(30);
        var token2 = "token2";
        var userId2 = Guid.NewGuid();
        var expiry2 = DateTime.UtcNow.AddMinutes(30);
        
        _sessionStore.AddSession(token1, userId1, expiry1);
        _sessionStore.AddSession(token2, userId2, expiry2);

        // Act
        var sessions = _sessionStore.GetAllSessions();

        // Assert
        Assert.Equal(2, sessions.Count()); // Should return both sessions
        Assert.Contains(sessions, s => s.UserId == userId1); // First session should be present
        Assert.Contains(sessions, s => s.UserId == userId2); // Second session should be present
    }

    [Fact]
    public void HasActiveSessionForUser_ShouldReturnTrueIfActiveSessionExists()
    {
        // Arrange
        var token = "test-token";
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddMinutes(30); // Valid expiry time
        _sessionStore.AddSession(token, userId, expiry);

        // Act
        var result = _sessionStore.HasActiveSessionForUser(userId);

        // Assert
        Assert.True(result); // Should return true because there is an active session
    }

    [Fact]
    public void HasActiveSessionForUser_ShouldReturnFalseIfNoActiveSessionExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = _sessionStore.HasActiveSessionForUser(userId);

        // Assert
        Assert.False(result); // Should return false because there is no active session
    }
}
