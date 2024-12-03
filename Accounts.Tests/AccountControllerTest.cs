using Accounts.API.Controllers;
using Accounts.API.DTO;
using Accounts.API.Services;
using Accounts.Core.Entities;
using Accounts.Core.Ports.Driven;
using Accounts.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Accounts.Tests
{
    public class AccountControllerTests : IAsyncLifetime
    {
        private readonly AccountController _controller;
        private readonly AccountService _accountService;
        private readonly SessionStore _sessionStore;
        private readonly AppDbContext _context;
        private readonly Mock<IValidator<RegisterUserRequest>> _validatorMock;

        public AccountControllerTests()
        {
            // Create the in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new AppDbContext(options);

            // Create mock for the validator
            _validatorMock = new Mock<IValidator<RegisterUserRequest>>();
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegisterUserRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Now we pass both the DbContext and the validator to the AccountService
            _accountService = new AccountService(_context, _validatorMock.Object);

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
            Assert.NotNull(createResult.UserId);
            Assert.NotEqual(Guid.Empty, createResult.UserId);

            // Act: Attempt to log in with the created credentials
            var loginRequest = new LoginRequest
            {
                Username = registerUser.Username,
                Password = registerUser.Password
            };

            var loginResponse = await _controller.Login(loginRequest);

            // Assert the response is of type OkObjectResult
            var loginOkResult = Assert.IsType<OkObjectResult>(loginResponse);

            // Cast the response to LoginResponse
            var loginResult = Assert.IsType<LoginResponse>(loginOkResult.Value);

            // Validates token and expiry
            Assert.NotNull(loginResult.Token);
            Assert.True(loginResult.Expiry > DateTime.UtcNow);

            // Ensure session is stored properly using TryGetSession
            var sessionExists = _sessionStore.TryGetSession(loginResult.Token, out var session);
            Assert.True(sessionExists);
            Assert.NotNull(session);
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
            Assert.NotNull(createResult.UserId);
            Assert.NotEqual(Guid.Empty, createResult.UserId);

            // Act: Attempt to log in with the created credentials
            var loginRequest = new LoginRequest
            {
                Username = registerUser.Username,
                Password = registerUser.Password
            };

            var loginResponse = await _controller.Login(loginRequest);

            // Assert the response is of type OkObjectResult
            var loginOkResult = Assert.IsType<OkObjectResult>(loginResponse);

            // Cast the response to LoginResponse
            var loginResult = Assert.IsType<LoginResponse>(loginOkResult.Value);

            // Validates token and expiry
            Assert.NotNull(loginResult.Token);
            Assert.True(loginResult.Expiry > DateTime.UtcNow);

            // Ensure session is stored properly using TryGetSession
            var sessionExists = _sessionStore.TryGetSession(loginResult.Token, out var session);
            Assert.True(sessionExists); // Check if the session exists
            Assert.NotNull(session); // Ensure the session is not null
            Assert.Equal(createResult.UserId, session.UserId); // Verify the UserId matches
            Assert.True(session.Expiry > DateTime.UtcNow); // Ensure the session expiry is in the future

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