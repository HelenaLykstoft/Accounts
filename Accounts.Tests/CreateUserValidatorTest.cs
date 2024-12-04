using Accounts.Core.Models;
using Accounts.Core.Validators;

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
            var user = new RegisterUserCommand
            {
                Username = "Johncena1" // Valid username
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Username_ShouldBeInvalid()
        {
            var user = new RegisterUserCommand
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
            var user = new RegisterUserCommand
            {
                Password = "Valid1Password@" // Valid password
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Password_ShouldBeInvalid()
        {
            var user = new RegisterUserCommand
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
            var user = new RegisterUserCommand
            {
                Email = "valid.email@example.com" // Valid email
                
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Email_ShouldBeInvalid()
        {
            var user = new RegisterUserCommand
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
            var user = new RegisterUserCommand
            {
                PhoneNumber =  "+4511111111" // Valid Danish phone number
            };

            var result = _createUserValidation.Validate(user);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void PhoneNumber_ShouldBeInvalid()
        {
            var user = new RegisterUserCommand
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
            var user = new RegisterUserCommand
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
            var user = new RegisterUserCommand
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
