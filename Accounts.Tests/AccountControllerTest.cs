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
        private readonly AppDbContext _context;

        public AccountControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new AppDbContext(options);
            _accountService = new AccountService(_context);
            _controller = new AccountController(_accountService);
        }

        
        public async Task InitializeAsync()
        {
            await SetUserTypes();
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
            // Arrange: Create a valid RegisterUserRequest
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
                UserTypeId = 2, // Ensure this user type exists
                Password = "ValidPassword123!"
            };

            // Act: Call the CreateUser method on the controller
            var response = await _controller.CreateUser(registerUser);

            // Assert: Check if the response is OK
            var okResult = Assert.IsType<OkObjectResult>(response);

            // Debugging: Output the response to see the structure
            Console.WriteLine($"Response Value: {okResult.Value}");

            // Attempt to cast the response to a dictionary for easy access
            var result = okResult.Value as IDictionary<string, object>;

            // Assert: Check if the result is not null and contains the "UserId" key
            Assert.NotNull(result); // Ensure the result is not null
            Assert.True(result.ContainsKey("UserId")); // Ensure it contains the "UserId" key
    
            // Access the UserId and assert it's not null
            var userId = result["UserId"];
            Assert.NotNull(userId); // Ensure UserId is not null
        }


    }
}