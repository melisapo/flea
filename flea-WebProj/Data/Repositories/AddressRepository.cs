using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IAddressRepository
{
    Task<List<Address>> GetByUserIdAsync(int userId);
    Task<Address?> GetByIdAsync(int addressId);
    Task<int> CreateAsync(Address address);
    Task<bool> UpdateAsync(Address address);
    Task<bool> DeleteAsync(int addressId);
}
public class AddressRepository(DatabaseContext dbContext) : IAddressRepository
{
    public async Task<List<Address>> GetByUserIdAsync(int userId)
    {
        const string query = """
                              SELECT id, city, state_province, country, user_id
                              FROM addresses
                              WHERE user_id = @userId
                              """;
        var parameters = new [] { new NpgsqlParameter("@userId", userId) };
        return await dbContext.ExecuteQueryAsync(query, MapAddress, parameters);
    }

    public async Task<Address?> GetByIdAsync(int addressId)
    {
        const string query = """
                             SELECT id, city, state_province, country, user_id
                             FROM addresses
                             WHERE id = @id
                             """;
        var parameters = new [] { new NpgsqlParameter("@id", addressId) };
        var addresses = await dbContext.ExecuteQueryAsync(query, MapAddress, parameters);
        return addresses.FirstOrDefault();
    }

    public async Task<int> CreateAsync(Address address)
    {
        const string query = """
                             INSERT INTO  addresses (id, city, state_province, country, user_id)
                             VALUES  (@id, @city, @state_province, @country, @user_id)
                             RETURNING id
                             """;
        address.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        var parameters = new[]
        {
            new NpgsqlParameter("@id", address.Id),
            new NpgsqlParameter( "@city", (object?)address.City ?? DBNull.Value),
            new NpgsqlParameter( "@state_province", address.StateProvince),
            new NpgsqlParameter( "@country", address.Country),
            new NpgsqlParameter( "@user_id", address.UserId )
        };
        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Address address)
    {
        const string query = """
                             UPDATE addresses
                             SET city = @city,
                                state_province = @state_province,
                                country = @country
                             WHERE id = @id
                             """;

        var parameters = new[]
        {
            new NpgsqlParameter("@id", address.Id),
            new NpgsqlParameter("@city", (object?)address.City ?? DBNull.Value),
            new NpgsqlParameter("@state_province", address.StateProvince),
            new NpgsqlParameter("@country", address.Country)
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int addressId)
    {
        const string query = "DELETE FROM addresses WHERE id = @id";
        var parameters = new[] { new NpgsqlParameter("@id", addressId) };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }
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
}