using Accounts.API.DTO;
using Accounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Accounts.Core.Entities;
using FluentValidation;
using Accounts.Core.Ports.Driven;

namespace Accounts.API.Services
{
    public class AccountService
    {
        private readonly AppDbContext _context;
        private readonly IValidator<RegisterUserRequest> _validator;
        public ISessionStore? SessionStore { get; set; }

        public AccountService(AppDbContext context, IValidator<RegisterUserRequest> validator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public virtual async Task<Guid> CreateUserAsync(RegisterUserRequest dto, 
            bool allowAdminCreation = false, bool isAdmin = false)
        {
            if (_context == null)
            {
                throw new InvalidOperationException("Database context is not initialized.");
            }

            // If not allowing admin creation and trying to create an admin user, throw exception
            if (dto.UserTypeId == 3 && !allowAdminCreation)
            {
                throw new UnauthorizedAccessException("Only an admin can create an admin user.");
            }

            // If an admin is not logged in and trying to create an admin user, deny
            if (dto.UserTypeId == 3 && !isAdmin)
            {
                throw new UnauthorizedAccessException("Only an admin can create an admin user.");
            }

            // Your validation code here
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

                // Handle userTypeId
                int userTypeId = dto.UserTypeId;

                // Check for admin creation, only admins can create admin users unless it's allowed explicitly
                if (userTypeId == 3 && !allowAdminCreation)
                {
                    throw new UnauthorizedAccessException("Only an admin can create another admin user.");
                }

                // Create user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Username = dto.Username,
                    UserTypeId = userTypeId,
                    ContactInfoId = contactInfo.Id
                };
                await _context.Users.AddAsync(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return user.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to create user: " + ex.Message, ex);
            }
        }



        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public async Task<int> GetUsersCountAsync()
        {
            return await _context.Users.CountAsync();
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

        public async Task SeedAdminUserAsync(string? token = null)
        {
            string username = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
            string password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");


            // Check if the user is admin
            bool isAdmin = token != null && await IsUserAdminAsync(token);

            // Skip token validation if no token is provided
            if (token != null && !isAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can perform this action.");
            }

            // Check if a main admin already exists
            var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.UserTypeId == 3);

            if (existingAdmin == null)
            {
                var adminUserRequest = new RegisterUserRequest
                {
                    FirstName = "Main",
                    LastName = "Admin",
                    Username = username,
                    Password = password,
                    UserTypeId = 3,
                    Email = "admin@example.com",
                    PhoneNumber = "12345678",
                    StreetNumber = 0,
                    StreetName = "Admin Lane",
                    PostalCode = 9999,
                    City = "AdminCity"
                };

                // Pass the `isAdmin` flag to CreateUserAsync
                await CreateUserAsync(adminUserRequest, allowAdminCreation: true, isAdmin);
            }
        }

        private async Task<bool> IsUserAdminAsync(string token)
        {
            if (SessionStore == null)
                throw new InvalidOperationException("SessionStore is not set.");

            if (!SessionStore.TryGetSession(token, out var session))
            {
                return false; // No session found
            }

            // Check if the session has expired
            if (session.Expiry <= DateTime.UtcNow)
            {
                SessionStore.RemoveSession(token); // Clean up expired session
                return false; // Session is no longer valid
            }

            // Verify the user's role
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
            return user?.UserTypeId == 3; // Assuming 3 is admin type
        }
    }
}