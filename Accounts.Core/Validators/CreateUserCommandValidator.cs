using FluentValidation;
using Accounts.Core.Models;

namespace Accounts.Core.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            // Username validation
            RuleFor(dto => dto.Username)
                .NotEmpty().WithMessage("Username cannot be empty.")
                .Matches("^[a-zA-Z0-9]{3,12}$")
                .WithMessage("Username must be between 3 and 12 characters and contain only letters and numbers.")
                .When(dto => !string.IsNullOrEmpty(dto.Username));

            // Password validation
            RuleFor(dto => dto.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]).{8,64}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one symbol, and be 8-64 characters long.")
                .When(dto => !string.IsNullOrEmpty(dto.Password));

            // Email validation
            RuleFor(dto => dto.Email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .Matches(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage("Invalid email format.")
                .When(dto => !string.IsNullOrEmpty(dto.Email));

            // Phone number validation (Danish format)
            RuleFor(dto => dto.PhoneNumber)
                .NotEmpty().WithMessage("Phone number cannot be empty.")
                .Matches(@"^(?:\+45\s?)?\d{2}\s?\d{2}\s?\d{2}\s?\d{2}$")
                .WithMessage("Phone number must be in Danish format.")
                .When(dto => !string.IsNullOrEmpty(dto.PhoneNumber));

            // Address validation
            RuleFor(dto => dto.StreetNumber)
                .NotEmpty().WithMessage("Streetnumber cannot be empty.")
                .GreaterThan(0).WithMessage("Street number must be greater than 0.")
                .When(dto => dto.StreetNumber > 0);

            RuleFor(dto => dto.StreetName)
                .NotEmpty().WithMessage("Streetname cannot be empty.")
                .Matches(@"^[A-Za-zæøåÆØÅ\s]+$")
                .WithMessage("Street name must only contain letters.")
                .When(dto => !string.IsNullOrEmpty(dto.StreetName));

            RuleFor(dto => dto.City)
                .NotEmpty().WithMessage("City name cannot be empty.")
                .When(dto => !string.IsNullOrEmpty(dto.City));

            RuleFor(dto => dto.PostalCode)
                .NotEmpty().WithMessage("Postal code cannot be empty.")
                .InclusiveBetween(1000, 9999).WithMessage("Postal code must be a 4-digit number.")
                .When(dto => dto.PostalCode > 0);
        }
    }
}
