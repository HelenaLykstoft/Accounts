using Accounts.Infrastructure.Persistence;
using Xunit;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        string password = "password123";

        // Act
        string hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword); // Ensure the hashed password is not null
        Assert.NotEqual(password, hashedPassword); // Ensure the hashed password is not equal to the plain password
        Assert.True(IsBase64String(hashedPassword)); // Optionally, verify that the result is base64
    }

    [Fact]
    public void HashPassword_SameInput_ShouldReturnSameHash()
    {
        // Arrange
        string password = "password123";

        // Act
        string hashedPassword1 = _passwordHasher.HashPassword(password);
        string hashedPassword2 = _passwordHasher.HashPassword(password);

        // Assert
        Assert.Equal(hashedPassword1, hashedPassword2); // Ensure the same input produces the same hash
    }

    private bool IsBase64String(string s)
    {
        // Check if the string is a valid base64 string
        Span<byte> buffer = new Span<byte>(new byte[s.Length * 3 / 4]);
        return Convert.TryFromBase64String(s, buffer, out _);
    }
}