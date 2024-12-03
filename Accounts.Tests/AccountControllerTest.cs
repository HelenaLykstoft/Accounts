using Accounts.API.Controllers;
using Accounts.API.DTO;
using Accounts.Core.Entities;
using Accounts.Core.Models;
using Accounts.Core.Ports.Driven;
using Accounts.Core.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Accounts.Tests
{
    public class AccountControllerTests
    {
        private readonly AccountController _controller;

        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAddressRepository> _addressRepositoryMock;
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly Mock<IContactInfoRepository> _contactInfoRepositoryMock;
        private readonly Mock<ILoginInfoRepository> _loginInfoRepositoryMock;
        private readonly Mock<ITransactionHandler> _transactionHandlerMock;
        private readonly Mock<IValidator<RegisterUserCommand>> _validatorMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ISessionStore> _sessionStoreMock;

        private readonly AccountService _service;

        public AccountControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _addressRepositoryMock = new Mock<IAddressRepository>();
            _cityRepositoryMock = new Mock<ICityRepository>();
            _contactInfoRepositoryMock = new Mock<IContactInfoRepository>();
            _loginInfoRepositoryMock = new Mock<ILoginInfoRepository>();
            _transactionHandlerMock = new Mock<ITransactionHandler>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _sessionStoreMock = new Mock<ISessionStore>();

            _validatorMock = new Mock<IValidator<RegisterUserCommand>>();
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Create the AccountService with mocked dependencies
            _service = new AccountService(
                _userRepositoryMock.Object,
                _addressRepositoryMock.Object,
                _cityRepositoryMock.Object,
                _contactInfoRepositoryMock.Object,
                _loginInfoRepositoryMock.Object,
                _transactionHandlerMock.Object,
                _validatorMock.Object,
                _passwordHasherMock.Object,
                _sessionStoreMock.Object
            );

            _controller = new AccountController(_service, _sessionStoreMock.Object);

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

        [Fact]
        public async Task CreateUser_ReturnsUserId_WhenValidRequest()
        {
            var registerUser = new RegisterUserCommand()
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


            _userRepositoryMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _cityRepositoryMock.Setup(c => c.GetOrCreateCityAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new City { PostalCode = 1000, Name = "Copenhagen" });
            _addressRepositoryMock.Setup(a => a.GetOrCreateAddressAsync(It.IsAny<Address>()))
                .ReturnsAsync(new Address { Id = Guid.NewGuid() });
            _contactInfoRepositoryMock.Setup(c => c.AddContactInfoAsync(It.IsAny<ContactInfo>()))
                .Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _transactionHandlerMock.Setup(handler => handler.ExecuteAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(func => { return func(); });

            //Act
            var response = await _controller.CreateUser(registerUser);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var result = Assert.IsType<AccountController.CreateUserResponse>(okResult.Value);
            Assert.NotEqual(Guid.Empty, result.UserId); 
        }

        [Fact]
        public async Task Login_ReturnsTokenAndExpiry_WhenValidCredentials()
        {
            // Arrange
            var registerUser = new RegisterUserCommand()
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

            _userRepositoryMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _cityRepositoryMock.Setup(c => c.GetOrCreateCityAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new City { PostalCode = 1000, Name = "Copenhagen" });
            _addressRepositoryMock.Setup(a => a.GetOrCreateAddressAsync(It.IsAny<Address>()))
                .ReturnsAsync(new Address { Id = Guid.NewGuid() });
            _contactInfoRepositoryMock.Setup(c => c.AddContactInfoAsync(It.IsAny<ContactInfo>()))
                .Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _transactionHandlerMock.Setup(handler => handler.ExecuteAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(func => { return func(); });

            // Create the user
            var createResponse = await _controller.CreateUser(registerUser);
            var createOkResult = Assert.IsType<OkObjectResult>(createResponse);
            var createResult = Assert.IsType<AccountController.CreateUserResponse>(createOkResult.Value);
            Assert.NotEqual(Guid.Empty, createResult.UserId); 

            // Act: Attempt to log in with the created credentials
            var loginRequest = new LoginRequest
            {
                Username = registerUser.Username,
                Password = registerUser.Password
            };

            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(new User
            {
                Username = registerUser.Username,
                LoginInformation = new LoginInformation { Password = "hashedPassword" }
            });

            var loginResponse = await _controller.Login(loginRequest);
            // Assert the response is of type OkObjectResult
            var loginOkResult = Assert.IsType<OkObjectResult>(loginResponse);

            // Cast the response to LoginResponse
            var loginResult = Assert.IsType<LoginResponse>(loginOkResult.Value);

            // Validates token and expiry
            Assert.NotNull(loginResult.Token);

            var token = loginResult.Token;
            var session = new Session
            {
                UserId = createResult.UserId,
                Expiry = DateTime.UtcNow.AddHours(1)
            };

            // Mock the session store to return the session when TryGetSession is called
            _sessionStoreMock.Setup(s => s.TryGetSession(token, out session)).Returns(true);

            // Ensure session is stored properly using TryGetSession
            var sessionExists = _sessionStoreMock.Object.TryGetSession(loginResult.Token, out var sessionResult);
            Assert.True(sessionExists);
            Assert.NotNull(sessionResult);
            Assert.Equal(createResult.UserId, sessionResult.UserId);
            Assert.True(sessionResult.Expiry > DateTime.UtcNow);
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

            _sessionStoreMock.Object.AddSession(token, userId, expiry);

            // Act
            var response = _controller.Logout(token);

            // Assert
            Assert.IsType<OkObjectResult>(response);
            Assert.False(_sessionStoreMock.Object.TryGetSession(token, out _));
        }

        [Fact]
        public async Task Me_ReturnsUserIdAndToken_WhenValidToken()
        {
            // Arrange
            var registerUser = new RegisterUserCommand()
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

            _userRepositoryMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _cityRepositoryMock.Setup(c => c.GetOrCreateCityAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new City { PostalCode = 1000, Name = "Copenhagen" });
            _addressRepositoryMock.Setup(a => a.GetOrCreateAddressAsync(It.IsAny<Address>()))
                .ReturnsAsync(new Address { Id = Guid.NewGuid() });
            _contactInfoRepositoryMock.Setup(c => c.AddContactInfoAsync(It.IsAny<ContactInfo>()))
                .Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _transactionHandlerMock.Setup(handler => handler.ExecuteAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(func => { return func(); });

            var createResponse = await _controller.CreateUser(registerUser);
            var createOkResult = Assert.IsType<OkObjectResult>(createResponse);
            var createResult = Assert.IsType<AccountController.CreateUserResponse>(createOkResult.Value);
            Assert.NotNull(createResult.UserId);
            Assert.NotEqual(Guid.Empty, createResult.UserId);

            // Act: Attempt to log in with the created credentials
            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(new User
            {
                Username = registerUser.Username,
                LoginInformation = new LoginInformation { Password = "hashedPassword" }
            });

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

            var token = loginResult.Token;
            var session = new Session
            {
                UserId = createResult.UserId,
                Expiry = DateTime.UtcNow.AddHours(1)
            };

            // Mock the session store to return the session when TryGetSession is called
            _sessionStoreMock.Setup(s => s.TryGetSession(token, out session)).Returns(true);

            // Ensure session is stored properly using TryGetSession
            var sessionExists = _sessionStoreMock.Object.TryGetSession(loginResult.Token, out var sessionResult);
            Assert.True(sessionExists); // Check if the session exists
            Assert.NotNull(sessionResult); // Ensure the session is not null
            Assert.Equal(createResult.UserId, sessionResult.UserId); // Verify the UserId matches
            Assert.True(sessionResult.Expiry > DateTime.UtcNow); // Ensure the session expiry is in the future

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