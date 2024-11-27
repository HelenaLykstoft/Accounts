using Accounts.API.DTO;
using Accounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Accounts.Domain.Entities;
using FluentValidation;

namespace Accounts.API.Services
{
    public class AccountService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<RegisterUserRequest> _validator;

        public AccountService(AppDbContext context, IValidator<RegisterUserRequest> validator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Guid> CreateUserAsync(RegisterUserRequest dto)
        {
            
            if (_context == null)
            {
                throw new InvalidOperationException("Database context is not initialized.");
            }
            
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if username already exists
                var existingLoginInfo = await _context.LoginInformations
                    .FirstOrDefaultAsync(li => li.Username == dto.Username);

                if (existingLoginInfo != null)
                {
                    throw new InvalidOperationException("Username is already taken.");
                }

                // Create or find the city
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.PostalCode == dto.PostalCode)
                       ?? new City { PostalCode = dto.PostalCode, Name = dto.City };
                if (city.Name != dto.City)
                {
                    city = new City
                    {
                        PostalCode = dto.PostalCode,
                        Name = dto.City
                    };
                    await _context.Cities.AddAsync(city);
                    await _context.SaveChangesAsync();
                }


                // Create address
                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    StreetNumber = dto.StreetNumber,
                    StreetName = dto.StreetName,
                    City = city
                };
                await _context.Addresses.AddAsync(address);

                // Create contact info
                var contactInfo = new ContactInfo
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    AddressId = address.Id
                };
                await _context.ContactInfos.AddAsync(contactInfo);

                // Hash password and create login info
                var passwordHash = HashPassword(dto.Password);
                var loginInfo = new LoginInformation
                {
                    Username = dto.Username,
                    Password = passwordHash
                };
                await _context.LoginInformations.AddAsync(loginInfo);

                // Create user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Username = dto.Username,
                    UserTypeId = 1,
                    ContactInfoId = contactInfo.Id
                };
                await _context.Users.AddAsync(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return user.Id;
            }
            catch ( Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to create user: " + ex.Message, ex);

            }
        }


        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users
                .Include(u => u.ContactInfo)
                .Include(u => u.UserType)
                .ToListAsync();
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            // Fetch the login information, including the associated User
            var loginInfo = await _context.LoginInformations
                .Include(li => li.User)
                .FirstOrDefaultAsync(li => li.Username == username);

            // Validate the password and return the associated User
            if (loginInfo == null || loginInfo.Password != HashPassword(password))
            {
                return null;
            }

            return loginInfo.User;
        }

        public async Task<string?> GetUsernameByIdAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Username;
        }
    }
}