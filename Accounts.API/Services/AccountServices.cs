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
            
            // Step 1: Create or find the City
            var city = await _context.Cities.FirstOrDefaultAsync(c => c.PostalCode == dto.PostalCode);
            if (city == null)
            {
                city = new City
                {
                    PostalCode = dto.PostalCode,
                    Name = dto.City // Assuming the city name is part of the DTO
                };
                await _context.Cities.AddAsync(city);
                await _context.SaveChangesAsync();
            }

            // Step 2: Create the Address and link it to the city (assign the City object, not just PostalCode)
            var address = new Address
            {
                Id = Guid.NewGuid(),
                StreetNumber = dto.StreetNumber,
                StreetName = dto.StreetName,
                City = city // Assign the entire City entity here
            };
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

            // Step 3: Create the ContactInfo and link it to the address
            var contactInfo = new ContactInfo
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                AddressId = address.Id // Link contact info to the created address using AddressId
            };
            await _context.ContactInfos.AddAsync(contactInfo);
            await _context.SaveChangesAsync();

            // Step 4: Create the Login Information (hashed password)
            var passwordHash = HashPassword(dto.Password); // Use a hashing function here
            var loginInfo = new LoginInformation
            {
                Username = dto.Username,
                Password = passwordHash // Store the hashed password
            };
            await _context.LoginInformations.AddAsync(loginInfo);
            await _context.SaveChangesAsync();

            // Step 5: Create the User and link it to contact info and login info
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                UserTypeId = dto.UserTypeId, // Link to UserType using the UserTypeId foreign key
                ContactInfoId = contactInfo.Id, // Link user to the created contact info using ContactInfoId
            };

            // Optionally check for valid UserType
            var userType = await _context.UserTypes.FirstOrDefaultAsync(ut => ut.Id == dto.UserTypeId);
            if (userType == null)
            {
                throw new ArgumentException("Invalid user type.");
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Return the newly created User ID
            return user.Id;
        }

        private string HashPassword(string password)
        {
            // Implement your password hashing logic here (e.g., SHA256, bcrypt)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)); // Placeholder hashing
        }
    }
}
