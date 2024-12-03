using Accounts.Core.Entities;
using Accounts.Core.Models;

namespace Accounts.Core.Ports.Driving
{
    public interface IAccountService
    {
        /// <summary>
        /// Creates a new user with the provided details.
        /// </summary>
        /// <param name="dto">Details of the user to create.</param>
        /// <param name="allowAdminCreation">Whether admin creation is allowed.</param>
        /// <param name="isAdmin">Indicates if the caller is an admin.</param>
        /// <returns>The ID of the newly created user.</returns>
        Task<Guid> CreateUserAsync(CreateUserCommand dto, bool allowAdminCreation = false, bool isAdmin = false);

        /// <summary>
        /// Validates a user's credentials.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The validated user or null if validation fails.</returns>
        Task<User?> ValidateUserAsync(string username, string password);

        /// <summary>
        /// Retrieves the username of a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The username of the user or null if not found.</returns>
        Task<string?> GetUsernameByIdAsync(Guid userId);

        /// <summary>
        /// Seeds an admin user if none exists.
        /// </summary>
        /// <param name="token">Optional token to verify admin privileges.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SeedAdminUserAsync(string? token = null);
    }
}
