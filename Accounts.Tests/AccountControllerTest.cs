using Accounts.API.Controllers;
using Accounts.API.DTO;
using Accounts.API.Services;
using Accounts.Domain.Entities;
using Accounts.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Accounts.Tests
{
    public class AccountControllerTests : IAsyncLifetime
    {
        private readonly AccountController _controller;
        private readonly AccountService _accountService;
        private readonly SessionStore _sessionStore;
        private readonly AppDbContext _context;

        public AccountControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new AppDbContext(options);
            _accountService = new AccountService(_context);
            _sessionStore = new SessionStore();
            _controller = new AccountController(_accountService, _sessionStore);

            // Mock HttpContext and ControllerContext
            var httpContextMock = new Mock<HttpContext>();
            var items = new Dictionary<object, object>();
            httpContextMock.Setup(ctx => ctx.Items).Returns(items);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            _controller.ControllerContext = controllerContext;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("Initializing user types...");
            await SetUserTypes();
            Console.WriteLine("User types initialized.");
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        private async Task SetUserTypes()
        {
            if (!_context.UserTypes.Any())
            {
                _context.UserTypes.AddRange(
                    new UserType() { Id = 1, Type = "user" },
                    new UserType { Id = 2, Type = "deliveryAgent" },
                    new UserType { Id = 3, Type = "admin" }
                );
                await _context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task CreateUser_ReturnsUserId_WhenValidRequest()
        {
            var registerUser = new RegisterUserRequest()
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "john.doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+4512345678",
                StreetNumber = 123,
                StreetName = "Main St",
                PostalCode = 1000,
                City = "Test City",
                UserTypeId = 2,
                Password = "ValidPassword123!"
            };

            var response = await _controller.CreateUser(registerUser);

            var okResult = Assert.IsType<OkObjectResult>(response);

            var result = Assert.IsType<AccountController.CreateUserResponse>(okResult.Value);

            Assert.NotNull(result.UserId);
            Assert.NotEqual(Guid.Empty, result.UserId);
        }

        [Fact]
        public async Task Login_ReturnsTokenAndExpiry_WhenValidCredentials()
        {
            // Arrange
            var registerUser = new RegisterUserRequest()
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "john.doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+4512345678",
                StreetNumber = 123,
                StreetName = "Main St",
                PostalCode = 1000,
                City = "Test City",
                UserTypeId = 2,
                Password = "ValidPassword123!"
            };

            // Create the user
            var createResponse = await _controller.CreateUser(registerUser);
            var createOkResult = Assert.IsType<OkObjectResult>(createResponse);
            var createResult = Assert.IsType<AccountController.CreateUserResponse>(createOkResult.Value);

            // Act
            var loginRequest = new LoginRequest
            {
                Username = registerUser.Username,
                Password = registerUser.Password
            };

            var loginResponse = await _controller.Login(loginRequest);

            // Assert
            var loginOkResult = Assert.IsType<OkObjectResult>(loginResponse);

            // Parses response into a concrete type
            var loginResult = loginOkResult.Value as IDictionary<string, object>;
            Assert.NotNull(loginResult);

            // Validates token and expiry
            Assert.True(loginResult.ContainsKey("Token"));
            Assert.True(loginResult.ContainsKey("Expiry"));

            var token = loginResult["Token"]?.ToString();
            var expiry = DateTime.Parse(loginResult["Expiry"]?.ToString() ?? string.Empty);

            Assert.NotNull(token);
            Assert.True(_sessionStore.TryGetSession(token, out var session));
            Assert.Equal(createResult.UserId, session.UserId);
            Assert.True(session.Expiry > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenInvalidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "invalid.user",
                Password = "WrongPassword"
            };

            // Act
            var response = await _controller.Login(loginRequest);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(response);
        }

        [Fact]
        public async Task Logout_RemovesSession_WhenValidToken()
        {
            // Arrange
            var token = "test-token";
            var userId = Guid.NewGuid();
            var expiry = DateTime.UtcNow.AddHours(1);

            _sessionStore.AddSession(token, userId, expiry);

            // Act
            var response = _controller.Logout(token);

            // Assert
            Assert.IsType<OkObjectResult>(response);
            Assert.False(_sessionStore.TryGetSession(token, out _));
        }

        [Fact]
        public async Task Me_ReturnsUserIdAndToken_WhenValidToken()
        {
            // Arrange
            var registerUser = new RegisterUserRequest()
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "john.doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+4512345678",
                StreetNumber = 123,
                StreetName = "Main St",
                PostalCode = 1000,
                City = "Test City",
                UserTypeId = 2,
                Password = "ValidPassword123!"
            };

            var createResponse = await _controller.CreateUser(registerUser);
            var createOkResult = Assert.IsType<OkObjectResult>(createResponse);
            var createResult = Assert.IsType<AccountController.CreateUserResponse>(createOkResult.Value);

            var loginRequest = new LoginRequest
            {
                Username = registerUser.Username,
                Password = registerUser.Password
            };

            var loginResponse = await _controller.Login(loginRequest);
            var loginOkResult = Assert.IsType<OkObjectResult>(loginResponse);
            var loginResult = Assert.IsType<LoginResponse>(loginOkResult.Value);

            // Act
            // Set HttpContext.Items to mock the session
            _controller.ControllerContext.HttpContext.Items["UserId"] = createResult.UserId;
            _controller.ControllerContext.HttpContext.Items["Token"] = loginResult.Token;

            var meResponse = _controller.Me();
            var meOkResult = Assert.IsType<OkObjectResult>(meResponse);
            var meResult = Assert.IsType<MeResponse>(meOkResult.Value);

            // Assert
            Assert.Equal(createResult.UserId, meResult.UserId);
            Assert.Equal(loginResult.Token, meResult.Token);
        }

        [Fact]
        public async Task Me_ReturnsUnauthorized_WhenInvalidToken()
        {
            // Arrange
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var response = _controller.Me();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(response);
        }
    }
}
