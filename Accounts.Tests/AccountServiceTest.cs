using Accounts.API.DTO;
using Accounts.API.Services;
using Accounts.Domain.Entities;
using Accounts.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Accounts.Tests
{
    public class AccountServiceTest : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly AccountService _service;
        private readonly Mock<IValidator<RegisterUserRequest>> _validatorMock;
        
        public AccountServiceTest()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;


            _dbContext = new AppDbContext(options);

            
            // Mock the validator
            _validatorMock = new Mock<IValidator<RegisterUserRequest>>();
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegisterUserRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            _service = new AccountService(_dbContext, _validatorMock.Object); 
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task CreateUser_ShouldAddUserToDatabase()
        {
            var userRequest = new RegisterUserRequest
            {
                FirstName = "Test",
                LastName = "Testing",
                Username = "johndoe",
                Password = "Valid1Password@",
                UserTypeId = 1,
                Email = "john.doe@example.com",
                PhoneNumber = "+4511111111",
                StreetNumber = 1,
                StreetName = "Main St",
                PostalCode = 1000,
                City = "Copenhagen"
            };

            // Call the CreateUserAsync method, which returns the user's ID
            var createdUserId = await _service.CreateUserAsync(userRequest);

            // Retrieve the user from the database using the created user's ID
            var addedUser = await _dbContext.Users
                .Include(u => u.ContactInfo)
                .ThenInclude(c => c.Address)
                .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.Id == createdUserId);

            // Assert the user was created successfully
            Assert.NotNull(addedUser);
            Assert.Equal(userRequest.Email, addedUser.ContactInfo.Email);
            Assert.Equal(userRequest.City, addedUser.ContactInfo.Address.City.Name);
        }


        [Fact]
        public async Task CreateUser_ShouldThrowException_ForInvalidUser()
        {
            
            var userRequest = new RegisterUserRequest
            {
                Username = "", 
                Password = "password", 
                Email = "invalid.email", 
                PhoneNumber = "12345", 
                StreetNumber = -1, 
                StreetName = "Main St",
                PostalCode = 999, 
                City = "Nowhere"
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(userRequest));
            Assert.Contains("Required properties", exception.Message);

        }
        

        [Fact]
        public async Task GetUsers_ShouldReturnAllUsers()
        {
            for (int i = 1; i <= 5; i++)
            {
                var userRequest = new RegisterUserRequest
                {
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Username = $"User{i}", 
                    Password = $"Passw{i}", 
                    Email = $"Mail{i}@example.com", 
                    PhoneNumber = $"2435675{i}", 
                    StreetNumber = i, 
                    StreetName = "Main St",
                    PostalCode = 9999, 
                    City = "Nowhere"
                };
                await _service.CreateUserAsync(userRequest);

            }

            await _dbContext.SaveChangesAsync();

            var result = await _service.GetUsersCountAsync();

            Assert.NotNull(result);
            Assert.Equal(5, result);
        }
    }
}
