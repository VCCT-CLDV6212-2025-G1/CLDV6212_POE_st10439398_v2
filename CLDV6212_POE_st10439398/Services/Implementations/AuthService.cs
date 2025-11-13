//-----------------------Start Of File----------------//
using CLDV6212_POE_st10439398.Models;
using CLDV6212_POE_st10439398.Models.ViewModels;
using CLDV6212_POE_st10439398.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CLDV6212_POE_st10439398.Services.Implementations
{

    /// Authentication service for user management
    /// Uses BCrypt for password hashing and Azure SQL Database for storage

    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlConnection")
                ?? throw new ArgumentNullException("SqlConnection string not found");
            _logger = logger;
        }


        /// Authenticates user with email and password

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt for non-existent email: {Email}", email);
                    return null;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user: {Email}", email);
                    return null;
                }

                // Verify password
                if (VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogInformation("Successful login for user: {Email}", email);
                    await UpdateLastLoginAsync(user.UserId);
                    return user;
                }

                _logger.LogWarning("Failed login attempt for user: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for email: {Email}", email);
                return null;
            }
        }


        /// Registers a new user

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Check if email already exists
                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "Email address is already registered", null);
                }

                // Hash the password
                var passwordHash = HashPassword(model.Password);

                // Create new user
                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Address = model.Address,
                    City = model.City,
                    Role = "Customer", // Default role
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Insert into database
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, Address, City, Role, IsActive, CreatedDate)
                    VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, @Address, @City, @Role, @IsActive, @CreatedDate);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address", (object?)user.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@City", (object?)user.City ?? DBNull.Value);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@IsActive", user.IsActive);
                command.Parameters.AddWithValue("@CreatedDate", user.CreatedDate);

                var userId = (int)await command.ExecuteScalarAsync();
                user.UserId = userId;

                _logger.LogInformation("New user registered: {Email} (UserId: {UserId})", user.Email, userId);

                return (true, "Registration successful", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", model.Email);
                return (false, "An error occurred during registration. Please try again.", null);
            }
        }


        /// Gets user by ID

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Users WHERE UserId = @UserId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUserFromReader(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return null;
            }
        }


        /// Gets user by email

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Users WHERE Email = @Email";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUserFromReader(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return null;
            }
        }


        /// Checks if email exists

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                return false;
            }
        }


        /// Updates last login date

        public async Task UpdateLastLoginAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "UPDATE Users SET LastLoginDate = @LoginDate WHERE UserId = @UserId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@LoginDate", DateTime.UtcNow);
                command.Parameters.AddWithValue("@UserId", userId);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            }
        }


        /// Hashes a password using BCrypt

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
        }


        /// Verifies a password against a BCrypt hash

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }


        /// Gets all users (admin function)

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Users ORDER BY CreatedDate DESC";
                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(MapUserFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
            }

            return users;
        }


        /// Updates user information

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE Users 
                    SET FirstName = @FirstName, 
                        LastName = @LastName, 
                        Phone = @Phone, 
                        Address = @Address, 
                        City = @City,
                        IsActive = @IsActive,
                        Role = @Role
                    WHERE UserId = @UserId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address", (object?)user.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@City", (object?)user.City ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", user.IsActive);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@UserId", user.UserId);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
                return false;
            }
        }


        /// Deactivates user account

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "UPDATE Users SET IsActive = 0 WHERE UserId = @UserId";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return false;
            }
        }


        /// Maps SqlDataReader to User object

        private User MapUserFromReader(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32("UserId"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                Role = reader.GetString("Role"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                LastLoginDate = reader.IsDBNull("LastLoginDate") ? null : reader.GetDateTime("LastLoginDate"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                City = reader.IsDBNull("City") ? null : reader.GetString("City")
            };
        }
    }
}
//-----------------------End Of File----------------//