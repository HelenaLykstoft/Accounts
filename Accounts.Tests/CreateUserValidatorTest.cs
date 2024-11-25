using Accounts.API.DTO;
using Accounts.API.Services;

namespace Accounts.UnitTests
{
    public class CreateUserValidatorTest
    {
        private readonly RegisterUserValidator _createUserValidation;

        public CreateUserValidatorTest()
        {
            _createUserValidation = new RegisterUserValidator();
        }

        // Username Validation
        [Fact]
        public void Username_ShouldBeValid()
        {
            var user = new RegisterUserRequest
            {
                Username = "Johncena1" // Valid username
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Username_ShouldBeInvalid()
        {
            var user = new RegisterUserRequest
            {
                Username = "jo" // Invalid: too short
            };

            var result = _createUserValidation.Validate(user);
            Assert.False(result.IsValid);
        }

        // Password Validation
        [Fact]
        public void Password_ShouldBeValid()
        {
            var user = new RegisterUserRequest
            {
                Password = "Valid1Password@" // Valid password
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Password_ShouldBeInvalid()
        {
            var user = new RegisterUserRequest
            {
                Password = "password" // Invalid: missing uppercase, number, symbol
            };

            var result = _createUserValidation.Validate(user);
            Assert.False(result.IsValid);
        }

        // Email Validation
        [Fact]
        public void Email_ShouldBeValid()
        {
            var user = new RegisterUserRequest
            {
                Email = "valid.email@example.com" // Valid email
                
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Email_ShouldBeInvalid()
        {
            var user = new RegisterUserRequest
            {
                Email = "valid.email@example" // Invalid email
                
            };

            var result = _createUserValidation.Validate(user);
            Assert.False(result.IsValid);
        }

        // Phone Number Validation
        [Fact]
        public void PhoneNumber_ShouldBeValid()
        {
            var user = new RegisterUserRequest
            {
                PhoneNumber =  "+4511111111" // Valid Danish phone number
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void PhoneNumber_ShouldBeInvalid()
        {
            var user = new RegisterUserRequest
            {
                PhoneNumber = "12345" // Invalid phone number format
            };

            var result = _createUserValidation.Validate(user);
            Assert.False(result.IsValid);
        }

        // Address Validation
        [Fact]
        public void Address_ShouldBeValid()
        {
            var user = new RegisterUserRequest
            {
                        StreetNumber = 1,
                        StreetName = "Main St",
                        PostalCode = 1000,
                        City = "Copenhagen",
            }; // Valid address data

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Address_ShouldBeInvalid()
        {
            var user = new RegisterUserRequest
            {
                StreetNumber = -2,
                StreetName = "Main St",
                PostalCode = 980,
                City = "Copenhagen",
            }; // Invalid address data

            var result = _createUserValidation.Validate(user);
            Assert.False(result.IsValid);
        }
    }
}
