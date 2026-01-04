using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> UpdateUsernameAsync(int userId, string newUsername);
    Task<bool> UpdateProfilePicAsync(int userId, string newProfilePic);
    Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
    Task<bool> DeleteAsync(int id);
    Task<User?> GetWithRolesAsync(int id);
    Task<List<User>> GetByRoleIdAsync(int roleId);
    Task<User?> GetFullUserAsync(int id);
}

public class UserRepository(DatabaseContext dbContext) : IUserRepository
{
    public async Task<List<User>> GetAllAsync()
    {
        const string query = """
                             SELECT id, username, name, password_hash, profile_pic, created_at, updated_at 
                             FROM users 
                             """;
        return await dbContext.ExecuteQueryAsync(query, MapUser);
    }

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
            VALUES (@id, @username, @name, @password_hash, @profile_pic, CURRENT_TIMESTAMP, @updated_at)
            RETURNING id
            """;

        user.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);

        var parameters = new[]
        {
            new NpgsqlParameter("@id", user.Id),
            new NpgsqlParameter("@username", user.Username),
            new NpgsqlParameter("@name", user.Name),
            new NpgsqlParameter("@password_hash", user.PasswordHash),
            new NpgsqlParameter("@profile_pic", user.ProfilePicture),
            new NpgsqlParameter("@created_at", user.CreatedAt),
            new NpgsqlParameter("@updated_at", (object?)user.UpdatedAt ?? DBNull.Value),
        };

        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateUsernameAsync(int userId, string newUsername)
    {
        const string checkQuery = """
                                  SELECT COUNT(*) FROM users 
                                  WHERE username = @username AND id != @id
                                  """;

        var checkParams = new[]
        {
            new NpgsqlParameter("@username", newUsername),
            new NpgsqlParameter("@id", userId)
        };

        var exists = Convert.ToInt64(await dbContext.ExecuteScalarAsync(checkQuery, checkParams)) > 0;
        if (exists)
            return false;

        const string query = """
                             UPDATE users 
                             SET username = @username, 
                                 updated_at = @updated_at
                             WHERE id = @id
                             """;

        var parameters = new[]
        {
            new NpgsqlParameter("@id", userId),
            new NpgsqlParameter("@username", newUsername),
            new NpgsqlParameter("@updated_at", DateTime.UtcNow)
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }


    public async Task<bool> UpdateAsync(User user)
    {
        const string query = """
            UPDATE users 
            SET name = @name, 
               updated_at = CURRENT_TIMESTAMP
            WHERE id = @id
            """;

        var parameters = new[]
        {
            new NpgsqlParameter("@id", user.Id),
            new NpgsqlParameter("@name", user.Name),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }
    
    public async Task<bool> UpdateProfilePicAsync(int userId, string newProfilePic)
    {
        const string query = """
                             UPDATE users 
                             SET profile_pic = @profile_pic, 
                                 updated_at = current_timestamp
                             WHERE id = @id
                             """;

        var parameters = new[]
        {
            new NpgsqlParameter("@id", userId),
            new NpgsqlParameter("@profile_pic", newProfilePic),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }
    
    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        const string query = """
                             UPDATE users 
                             SET password_hash = @password_hash, 
                                 updated_at = @updated_at
                             WHERE id = @id
                             """;

        var parameters = new[]
        {
            new NpgsqlParameter("@id", userId),
            new NpgsqlParameter("@password_hash", newPasswordHash),
            new NpgsqlParameter("@updated_at", DateTime.UtcNow)
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
        if (user == null)
            return null;

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
        var addressRepository = new AddressRepository(dbContext);
        var contactRepository = new ContactRepository(dbContext);
        
        var user = await GetWithRolesAsync(id);
        if (user == null)
            return null;
        user.Address = await addressRepository.GetByUserIdAsync(id);
        user.Contact = await contactRepository.GetByUserIdAsync(id);

        return user;
    }

    private static User MapUser(NpgsqlDataReader reader) 
        => new()
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Name = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            ProfilePicture = reader.GetString(4),
            CreatedAt = reader.GetDateTime(5),
            UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
        };

    private static Role MapRole(NpgsqlDataReader reader) 
        => new()
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1)
        };
    
}
