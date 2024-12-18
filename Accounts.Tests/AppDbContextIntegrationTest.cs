using Microsoft.EntityFrameworkCore;
using Accounts.Infrastructure.Persistence;
using Xunit;
using System;
using System.Threading.Tasks;
using Accounts.Core.Entities;
using Accounts.Core.Models;

namespace Accounts.Tests
{
    public class AppDbContextTests : IDisposable
    {
        private readonly AppDbContext _context;

        public AppDbContextTests()
        {
            // Dynamic PostgreSQL test database
            var testDatabaseName = $"testDb_{Guid.NewGuid()}"; // Unique database name for each test run
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(
                    $"Host=localhost;Port=5433;Database={testDatabaseName};Username=postgres;Password=postgres;")
                .Options;

            _context = new AppDbContext(options);

            // Ensure database is created
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            // Cleanup the test database after the test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task TestAddDefaultUserTypesAsync()
        {
            // Arrange
            await _context.AddDefaultUserTypesAsync();

            // Act
            var userTypes = await _context.UserTypes.ToListAsync();

            // Assert
            Assert.Equal(3, userTypes.Count); // Ensure 3 default user types are added
            Assert.Contains(userTypes, ut => ut.Type == "user");
            Assert.Contains(userTypes, ut => ut.Type == "deliveryAgent");
            Assert.Contains(userTypes, ut => ut.Type == "admin");
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldRegisterUserSuccessfully()
        {
            // Arrange - Prepare the RegisterUserCommand
            var registerUserCommand = new RegisterUserCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "john_doe",
                Password = "password123", // This should now work as Password is a property of User
                UserTypeId = 1, // Assuming 1 is the valid UserTypeId
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                StreetNumber = 123,
                StreetName = "Main St",
                PostalCode = 10001,
                City = "New York"
            };

            // First, create the City
            var city = new City
            {
                PostalCode = registerUserCommand.PostalCode,
                Name = registerUserCommand.City
            };

            // Add the city to the database
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            // Then, create the Address and ensure it references the correct City
            var address = new Address
            {
                StreetNumber = registerUserCommand.StreetNumber,
                StreetName = registerUserCommand.StreetName,
                CityPostalCode = city.PostalCode // Ensure this matches the City
            };

            // Add the address to the database
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync(); // Ensure the Address is saved and has an ID

            // Now, create the ContactInfo with the AddressId set to the newly created Address
            var contactInfo = new ContactInfo
            {
                Email = registerUserCommand.Email,
                PhoneNumber = registerUserCommand.PhoneNumber,
                AddressId = address.Id // The AddressId must reference the correct Address
            };

            // Add ContactInfo to the database
            _context.ContactInfos.Add(contactInfo);
            await _context.SaveChangesAsync();

            // Create the LoginInformation first, ensuring the Username exists before User
            var loginInformation = new LoginInformation
            {
                Username = registerUserCommand.Username,
                Password = registerUserCommand.Password // You can store encrypted passwords here
            };

            // Add the LoginInformation to the database
            _context.LoginInformations.Add(loginInformation);
            await _context.SaveChangesAsync();

            // Now, create the user with the related ContactInfoId and UserTypeId
            var user = new User
            {
                FirstName = registerUserCommand.FirstName,
                LastName = registerUserCommand.LastName,
                Username = registerUserCommand.Username, // This matches the LoginInformation's Username
                UserTypeId = registerUserCommand.UserTypeId,
                ContactInfoId = contactInfo.Id // Ensure this is linked to the correct ContactInfo
            };

            // Add the user to the database
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Assert - Verify that the user was created correctly
            var addedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerUserCommand.Username);
            Assert.NotNull(addedUser);
            Assert.Equal(registerUserCommand.FirstName, addedUser.FirstName);
            Assert.Equal(registerUserCommand.LastName, addedUser.LastName);
            Assert.Equal(registerUserCommand.Email, addedUser.ContactInfo.Email);
            Assert.Equal(registerUserCommand.PhoneNumber, addedUser.ContactInfo.PhoneNumber);
            Assert.Equal(registerUserCommand.StreetNumber, addedUser.ContactInfo.Address.StreetNumber);
            Assert.Equal(registerUserCommand.StreetName, addedUser.ContactInfo.Address.StreetName);
            Assert.Equal(registerUserCommand.City, addedUser.ContactInfo.Address.City.Name);
            Assert.Equal(registerUserCommand.PostalCode, addedUser.ContactInfo.Address.City.PostalCode);
        }
    }
}