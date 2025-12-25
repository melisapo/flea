using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<User?> GetWithRolesAsync(int id);
    Task<List<User>> GetByRoleIdAsync(int roleId);
    Task<User?> GetFullUserAsync(int id);
    Task<List<Post>> GetUserPostsAsync(int userId, int limit = 10);
}

public class UserRepository(DatabaseContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id)
        {
            const string query = """
                                 SELECT id, username, name, password_hash, profile_pic, created_at, updated_at 
                                 FROM users 
                                 WHERE id = @id
                                 """;

            var parameters = new[] { new NpgsqlParameter("@id", id) };
            var users = await dbContext.ExecuteQueryAsync(query, MapUser, parameters);
            return users.FirstOrDefault();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string query = """
                                 SELECT id, username, name, password_hash, profile_pic, created_at, updated_at 
                                 FROM users 
                                 WHERE username = @username
                                 """;

            var parameters = new[] { new NpgsqlParameter("@username", username) };
            var users = await dbContext.ExecuteQueryAsync(query, MapUser, parameters);
            return users.FirstOrDefault();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            const string query = "SELECT COUNT(*) FROM users WHERE username = @username";
            var parameters = new[] { new NpgsqlParameter("@username", username) };
            var result = await dbContext.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(result) > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string query = "SELECT COUNT(*) FROM contacts WHERE email = @email";
            var parameters = new[] { new NpgsqlParameter("@email", email) };
            var result = await dbContext.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(result) > 0;
        }

        public async Task<int> CreateAsync(User user)
        {
            const string query = """
                                  INSERT INTO users (id, username, name, password_hash, profile_pic, created_at, updated_at)
                                  VALUES (@id, @username, @name, @password_hash, @profile_pic, @created_at, @updated_at)
                                  RETURNING id
                                  """;

            user.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
            user.CreatedAt = DateTime.UtcNow;

            var parameters = new[]
            {
                new NpgsqlParameter("@id", user.Id),
                new NpgsqlParameter("@username", user.Username),
                new NpgsqlParameter("@name", user.Name),
                new NpgsqlParameter("@password_hash", user.PasswordHash),
                new NpgsqlParameter("@profile_pic", user.ProfilePicture),
                new NpgsqlParameter("@created_at", user.CreatedAt),
                new NpgsqlParameter("@updated_at", (object?)user.UpdatedAt ?? DBNull.Value)
            };

            var result = await dbContext.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            const string query = """
                                 UPDATE users 
                                 SET name = @name, 
                                    profile_pic = @profile_pic, 
                                    updated_at = @updated_at
                                 WHERE id = @id
                                 """;

            user.UpdatedAt = DateTime.UtcNow;

            var parameters = new[]
            {
                new NpgsqlParameter("@id", user.Id),
                new NpgsqlParameter("@name", user.Name),
                new NpgsqlParameter("@profile_pic", user.ProfilePicture),
                new NpgsqlParameter("@updated_at", user.UpdatedAt)
            };

            var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM users WHERE id = @id";
            var parameters = new[] { new NpgsqlParameter("@id", id) };
            var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<User?> GetWithRolesAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return null;

            const string query = """
                                 SELECT r.id, r.name
                                 FROM roles r
                                 INNER JOIN user_roles ur ON r.id = ur.role_id
                                 WHERE ur.user_id = @userId
                                 """;

            var parameters = new[] { new NpgsqlParameter("@userId", id) };
            user.Roles = await dbContext.ExecuteQueryAsync(query, MapRole, parameters);
            return user;
        }
        
        public async Task<List<User>> GetByRoleIdAsync(int roleId)
        {
            const string query = """
                                 SELECT u.id, u.username, u.name, u.created_at
                                 FROM users u
                                 JOIN user_roles ur ON u.id = ur.user_id
                                 WHERE ur.role_id = @roleId;
                                 """;
            var parameters = new[] { new NpgsqlParameter("@roleId", roleId) };
            return await dbContext.ExecuteQueryAsync(query, MapUser, parameters);
        }

        public async Task<User?> GetFullUserAsync(int id)
        {
            var user = await GetWithRolesAsync(id);
            if (user == null) return null;
            
            const string addressQuery = """
                                        SELECT id, city, state_province, country, user_id
                                        FROM addresses
                                        WHERE user_id = @userId
                                        """;

            var addressParams = new[] { new NpgsqlParameter("@userId", id) };
            user.Addresses = await dbContext.ExecuteQueryAsync(addressQuery, MapAddress, addressParams);
            
            const string contactQuery = """
                                        SELECT id, email, phone_number, telegram_user, user_id
                                        FROM contacts
                                        WHERE user_id = @userId
                                        """;

            var contactParams = new[] { new NpgsqlParameter("@userId", id) };
            var contacts = await dbContext.ExecuteQueryAsync(contactQuery, MapContact, contactParams);
            user.Contact = contacts.FirstOrDefault();

            return user;
        }

        public async Task<List<Post>> GetUserPostsAsync(int userId, int limit = 10)
        {
            const string query = """
                                 SELECT p.id, p.title, p.description, p.created_at, p.updated_at, p.product_id, p.author_id
                                 FROM posts p
                                 WHERE p.author_id = @userId
                                 ORDER BY p.created_at DESC
                                 LIMIT @limit
                                 """;

            var parameters = new[]
            {
                new NpgsqlParameter("@userId", userId),
                new NpgsqlParameter("@limit", limit)
            };

            return await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
        }

        private static User MapUser(NpgsqlDataReader reader)
            => new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Name = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                ProfilePicture = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5),
                UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
            };
        

        private static Role MapRole(NpgsqlDataReader reader)
            => new Role
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        

        private static Address MapAddress(NpgsqlDataReader reader)
        {
            return new Address
            {
                Id = reader.GetInt32(0),
                City = reader.IsDBNull(1) ? null : reader.GetString(1),
                StateProvince = reader.GetString(2),
                Country = reader.GetString(3),
                UserId = reader.GetInt32(4)
            };
        }

        private static Contact MapContact(NpgsqlDataReader reader)
            => new Contact
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                PhoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                TelegramUser = reader.IsDBNull(3) ? null : reader.GetString(3),
                UserId = reader.GetInt32(4)
            };

        private static Post MapPost(NpgsqlDataReader reader)
            => new Post
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3),
                UpdatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                ProductId = reader.GetInt32(5),
                AuthorId = reader.GetInt32(6)
            };
    
}