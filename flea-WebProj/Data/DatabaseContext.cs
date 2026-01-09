using System.Data;
using Npgsql;

namespace flea_WebProj.Data;

public class DatabaseContext(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
                                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Método para obtener una nueva conexión
    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    // Método para ejecutar queries que devuelven datos (SELECT)
    public async Task<List<T>> ExecuteQueryAsync<T>(string query, Func<NpgsqlDataReader, T> mapFunction,
        NpgsqlParameter[]? parameters = null)
    {
        var results = new List<T>();

        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);

        if (parameters != null) command.Parameters.AddRange(parameters);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) results.Add(mapFunction(reader));

        return results;
    }

    // Método para ejecutar queries que NO devuelven datos (INSERT, UPDATE, DELETE)
    public async Task<int> ExecuteNonQueryAsync(string query, NpgsqlParameter[]? parameters = null)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);

        if (parameters != null) command.Parameters.AddRange(parameters);

        return await command.ExecuteNonQueryAsync();
    }

    // Método para ejecutar queries que devuelven un solo valor (COUNT, MAX, etc.)
    public async Task<object?> ExecuteScalarAsync(string query, NpgsqlParameter[]? parameters = null)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);

        if (parameters != null) command.Parameters.AddRange(parameters);

        return await command.ExecuteScalarAsync();
    }

    // Método para ejecutar transacciones (cuando necesita hacer múltiples operaciones)
    public async Task ExecuteTransactionAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task> action)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await action(connection, transaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}