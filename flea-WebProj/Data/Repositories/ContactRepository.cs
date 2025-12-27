using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IContactRepository
{
    Task<Contact?> GetByUserIdAsync(int userId);
    Task<int> CreateAsync (Contact contact);
    Task<bool> UpdateAsync (Contact contact);
    Task<bool> DeleteAsync(int id);
}

public class ContactRepository(DatabaseContext dbContext) : IContactRepository
{
    public async Task<Contact?> GetByUserIdAsync(int userId)
    {
        const string query = """
                             SELECT id, email, phone_number, telegram_user, user_id
                             FROM contacts
                             WHERE user_id = @userId";"
                             """;
        var parameters = new[] { new NpgsqlParameter("@userId", userId) };
        var contacts = await dbContext.ExecuteQueryAsync(query, MapContact, parameters);
        return contacts.FirstOrDefault();
    }
    
    public async Task<int> CreateAsync(Contact contact)
    {
        const string query = """
                             INSERT INTO contacts  (id, email, phone_number, telegram_user, user_id)
                             VALUES (@id,  @email, @phone_number, @telegram_user, @user_id)
                             RETURNING id
                             """;

        contact.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        var parameters = new[]
        {
            new NpgsqlParameter("@id", contact.Id),
            new NpgsqlParameter("@email", contact.Email),
            new NpgsqlParameter("@phone_number", (object?)contact.PhoneNumber ?? DBNull.Value),
            new NpgsqlParameter("@telegram_user", (object?)contact.TelegramUser ?? DBNull.Value),
            new NpgsqlParameter("@user_id", contact.UserId)
        };
        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Contact contact)
    {
        const string query = """
                             UPDATE contacts
                             SET email  = @email,
                                 phone_number = @phone_number,
                                 telegram_user = @telegram_user,
                             WHERE id = @id";"
                             """;
        var parameters = new[]
        {
            new NpgsqlParameter("@id", contact.Id),
            new NpgsqlParameter("@email", contact.Email),
            new NpgsqlParameter("@phone_number", (object?)contact.PhoneNumber ?? DBNull.Value),
            new NpgsqlParameter("@telegram_user", (object?)contact.TelegramUser ?? DBNull.Value),
        };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string query = "DELETE FROM contacts WHERE id = @id";
        var parameters = new [] {new NpgsqlParameter("@id", id)};
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }
    
    private static Contact MapContact(NpgsqlDataReader reader) 
        => new()
        {
            Id = reader.GetInt32(0),
            Email = reader.GetString(1),
            PhoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
            TelegramUser = reader.IsDBNull(3) ? null : reader.GetString(3),
            UserId = reader.GetInt32(4)
        };
}