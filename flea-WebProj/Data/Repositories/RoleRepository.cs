using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<List<Role>> GetAllAsync();
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<List<Role>> GetUserRolesAsync(int userId);
}

public class RoleRepository(DatabaseContext dbContext) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(int id)
    {
        const string query = "SELECT id, name FROM roles WHERE id = @id";
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var roles = await dbContext.ExecuteQueryAsync(query, MapRole, parameters);
        return roles.FirstOrDefault();
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        const string query = "SELECT id, name FROM roles WHERE name = @name";
        var parameters = new[] { new NpgsqlParameter("@name", name) };
        var roles = await dbContext.ExecuteQueryAsync(query, MapRole, parameters);
        return roles.FirstOrDefault();
    }

    public async Task<List<Role>> GetAllAsync()
    {
        const string query = "SELECT id, name FROM roles ORDER BY name";
        return await dbContext.ExecuteQueryAsync(query, MapRole);
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        // Verificar si ya existe la relación
        const string checkQuery =
            """
            SELECT COUNT(*) FROM user_roles 
            WHERE user_id = @userId AND role_id = @roleId
            """;

        var checkParams = new[]
        {
            new NpgsqlParameter("@userId", userId),
            new NpgsqlParameter("@roleId", roleId),
        };

        var exists =
            Convert.ToInt64(await dbContext.ExecuteScalarAsync(checkQuery, checkParams)) > 0;
        if (exists)
            return true;
        
        const string insertQuery =
            """
            INSERT INTO user_roles (id, user_id, role_id)
            VALUES (@id, @userId, @roleId)
            """;

        var id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        var insertParams = new[]
        {
            new NpgsqlParameter("@id", id),
            new NpgsqlParameter("@userId", userId),
            new NpgsqlParameter("@roleId", roleId),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(insertQuery, insertParams);
        return rowsAffected > 0;
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        const string query =
            """
            DELETE FROM user_roles 
            WHERE user_id = @userId AND role_id = @roleId
            """;

        var parameters = new[]
        {
            new NpgsqlParameter("@userId", userId),
            new NpgsqlParameter("@roleId", roleId),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<List<Role>> GetUserRolesAsync(int userId)
    {
        const string query =
            """
            SELECT r.id, r.name
            FROM roles r
            INNER JOIN user_roles ur ON r.id = ur.role_id
            WHERE ur.user_id = @userId
            """;

        var parameters = new[] { new NpgsqlParameter("@userId", userId) };
        return await dbContext.ExecuteQueryAsync(query, MapRole, parameters);
    }

    private static Role MapRole(NpgsqlDataReader reader) 
        => new Role
        {
            Id = reader.GetInt32(0), 
            Name = reader.GetString(1)
        };
    
}