using Accounts.Core.Entities;
using Accounts.Core.Models;
using Accounts.Core.Ports.Driven;
using Accounts.Core.Ports.Driving;
using FluentValidation;

namespace Accounts.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IContactInfoRepository _contactInfoRepository;
        private readonly ILoginInfoRepository _loginInfoRepository;
        private readonly ITransactionHandler _transactionHandler;
        private readonly IValidator<CreateUserCommand> _validator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ISessionStore _sessionStore;

        public AccountService(
            IUserRepository userRepository,
            IAddressRepository addressRepository,
            ICityRepository cityRepository,
            IContactInfoRepository contactInfoRepository,
            ILoginInfoRepository loginInfoRepository,
            ITransactionHandler transactionHandler,
            IValidator<CreateUserCommand> validator,
            IPasswordHasher passwordHasher,
            ISessionStore sessionStore)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
            _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
            _contactInfoRepository = contactInfoRepository ?? throw new ArgumentNullException(nameof(contactInfoRepository));
            _loginInfoRepository = loginInfoRepository ?? throw new ArgumentNullException(nameof(loginInfoRepository));
            _transactionHandler = transactionHandler ?? throw new ArgumentNullException(nameof(transactionHandler));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        }

        public async Task<Guid> CreateUserAsync(CreateUserCommand dto, bool allowAdminCreation = false, bool isAdmin = false)
        {
            // Validation
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Admin checks
            if (dto.UserTypeId == 3 && (!allowAdminCreation || !isAdmin))
            {
                throw new UnauthorizedAccessException("Only an admin can create an admin user.");
            }

            var createdUserId = Guid.Empty;

            try
            {
                // Transactional creation of user
                await _transactionHandler.ExecuteAsync(async () =>
                {
                    if (await _userRepository.UsernameExistsAsync(dto.Username))
                    {
                        throw new InvalidOperationException("Username is already taken.");
                    }

                    var city = await _cityRepository.GetOrCreateCityAsync(dto.PostalCode, dto.City);
                    var address = await _addressRepository.GetOrCreateAddressAsync(new Address
                    {
                        StreetNumber = dto.StreetNumber,
                        StreetName = dto.StreetName,
                        City = city
                    });

                    var contactInfo = new ContactInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        AddressId = address.Id
                    };
                    await _contactInfoRepository.AddContactInfoAsync(contactInfo);

                    var hashedPassword = _passwordHasher.HashPassword(dto.Password);
                    var loginInfo = new LoginInformation
                    {
                        Username = dto.Username,
                        Password = hashedPassword
                    };

                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Username = dto.Username,
                        UserTypeId = dto.UserTypeId,
                        ContactInfo = contactInfo,
                        LoginInformation = loginInfo
                    };

                    await _userRepository.AddUserAsync(user);

                    createdUserId = user.Id;
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create user: " + ex.Message, ex);
            }

            return createdUserId;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null || user.LoginInformation.Password != _passwordHasher.HashPassword(password))
            {
                return null;
            }

            return user;
        }

        public async Task<string?> GetUsernameByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            return user?.Username;
        }

        public async Task SeedAdminUserAsync(string? token = null)
        {
            if (await _userRepository.AdminAccountExistsAsync())
            {
                return; // Doesn't create seed admin if admin already exists
            }

            var username = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
            var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Admin credentials are not set in the environment variables.");
            }

            bool isAdmin = token != null && await IsUserAdminAsync(token);

            if (token != null && !isAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can perform this action.");
            }

            var adminUserRequest = new CreateUserCommand
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

            await CreateUserAsync(adminUserRequest, allowAdminCreation: true, isAdmin: true);
        }

        private async Task<bool> IsUserAdminAsync(string token)
        {
            if (!_sessionStore.TryGetSession(token, out var session))
            {
                return false; // No session found
            }

            if (session.Expiry <= DateTime.UtcNow)
            {
                _sessionStore.RemoveSession(token);
                return false;
            }

            var user = await _userRepository.GetUserByIdAsync(session.UserId);
            return user?.UserTypeId == 3;
        }
    }
}