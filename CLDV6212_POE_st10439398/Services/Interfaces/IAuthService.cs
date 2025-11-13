//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Models.ViewModels;

namespace CLDV6212_POE_st10439398.Services.Interfaces
{

    /// Interface for authentication service
    /// Handles user registration, login, and password management

    public interface IAuthService
    {
 
        /// Authenticates a user with email and password
        Task<User?> AuthenticateAsync(string email, string password);

        /// Registers a new user
        Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model);

        /// Gets a user by their ID
        Task<User?> GetUserByIdAsync(int userId);

        /// Gets a user by their email
        Task<User?> GetUserByEmailAsync(string email);

        /// Checks if an email is already registered
        Task<bool> EmailExistsAsync(string email);

        /// Updates user's last login date
        Task UpdateLastLoginAsync(int userId);

        /// Hashes a password using BCrypt
        string HashPassword(string password);

        /// Verifies a password against a hash
        bool VerifyPassword(string password, string hash);

        /// Gets all users (admin only)
        Task<List<User>> GetAllUsersAsync();

        /// Updates user information
        Task<bool> UpdateUserAsync(User user);

        /// Deactivates a user account
        Task<bool> DeactivateUserAsync(int userId);
    }
}
//-----------------------End Of File----------------//