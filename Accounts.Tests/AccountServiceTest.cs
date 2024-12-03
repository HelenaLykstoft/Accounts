using Accounts.Core.Entities;
using Accounts.Core.Models;
using Accounts.Core.Ports.Driven;
using Accounts.Core.Ports.Driving;
using Accounts.Core.Services;
using Accounts.Core.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Accounts.Tests
{
    public class AccountServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAddressRepository> _addressRepositoryMock;
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly Mock<IContactInfoRepository> _contactInfoRepositoryMock;
        private readonly Mock<ILoginInfoRepository> _loginInfoRepositoryMock;
        private readonly Mock<ITransactionHandler> _transactionHandlerMock;
        //private readonly Mock<IValidator<RegisterUserCommand>> _validatorMock;
        private readonly RegisterUserValidator _validator;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ISessionStore> _sessionStoreMock;

        private readonly AccountService _service;

        public AccountServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _addressRepositoryMock = new Mock<IAddressRepository>();
            _cityRepositoryMock = new Mock<ICityRepository>();
            _contactInfoRepositoryMock = new Mock<IContactInfoRepository>();
            _loginInfoRepositoryMock = new Mock<ILoginInfoRepository>();
            _transactionHandlerMock = new Mock<ITransactionHandler>();
            //_validatorMock = new Mock<IValidator<RegisterUserCommand>>();
            _validator = new RegisterUserValidator();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _sessionStoreMock = new Mock<ISessionStore>();

            //_validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RegisterUserCommand>(), default))
            //    .ReturnsAsync(new ValidationResult());
            
            _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");

            _service = new AccountService(
                _userRepositoryMock.Object,
                _addressRepositoryMock.Object,
                _cityRepositoryMock.Object,
                _contactInfoRepositoryMock.Object,
                _loginInfoRepositoryMock.Object,
                _transactionHandlerMock.Object,
                _validator,
                _passwordHasherMock.Object,
                _sessionStoreMock.Object
            );
        }

        [Fact]
        public async Task CreateUser_ShouldAddUserToDatabase()
        {
            var userRequest = new RegisterUserCommand
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
                .Returns<Func<Task>>(func =>
                {
                    return func();
                });

            // Act
            var createdUserId = await _service.CreateUserAsync(userRequest);

            // Assert
            _userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);  
            Assert.NotEqual(Guid.Empty, createdUserId);
        }


        [Fact]
        public async Task CreateUser_ShouldThrowException_ForInvalidUser()
        {
            
            var userRequest = new RegisterUserCommand
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

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateUserAsync(userRequest));
            Assert.Contains("Invalid email format.", exception.Message);
        }
    }
}
