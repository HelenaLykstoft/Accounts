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

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateUserAsync(RegisterUserRequest dto)
        {
            
            var existingLoginInfo = await _context.LoginInformations
                .FirstOrDefaultAsync(li => li.Username == dto.Username);

            if (existingLoginInfo != null)
            {
                throw new InvalidOperationException("Username is already taken.");
            }
            
            // Creates or finds the city
            var city = await _context.Cities.FirstOrDefaultAsync(c => c.PostalCode == dto.PostalCode);
            if (city == null)
            {
                city = new City
                {
                    PostalCode = dto.PostalCode,
                    Name = dto.City
                };
                await _context.Cities.AddAsync(city);
                await _context.SaveChangesAsync();
            }

            // Creates address and links to city
            var address = new Address
            {
                Id = Guid.NewGuid(),
                StreetNumber = dto.StreetNumber,
                StreetName = dto.StreetName,
                City = city
            };
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

            // Creates the ContactInfo and link it to the address
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                AddressId = address.Id 
            };
            await _context.ContactInfos.AddAsync(contactInfo);
            await _context.SaveChangesAsync();

            // Creates the Login Information (hashed password)
            // Might needs changing compared to how we are gonna handle login
            var passwordHash = HashPassword(dto.Password);
            var loginInfo = new LoginInformation
            {
                Username = dto.Username,
                Password = passwordHash 
            };
            await _context.LoginInformations.AddAsync(loginInfo);
            await _context.SaveChangesAsync();

            // Creates the User and link it to contact info and login info
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                UserTypeId = dto.UserTypeId, 
                ContactInfoId = contactInfo.Id,
            };

            // Checks for valid UserType
            var userType = await _context.UserTypes.FirstOrDefaultAsync(ut => ut.Id == dto.UserTypeId);
            if (userType == null)
            {
                throw new ArgumentException("Invalid user type.");
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // The return. Right now it returns the user id when a user is created.
            return user.Id;
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

    }
}
