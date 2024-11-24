using Accounts.API.Controllers;
using Accounts.API.DTO;
using Accounts.API.Services;
using Accounts.Domain.Entities;
using Accounts.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    }
}