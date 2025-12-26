using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IImageRepository
{
    Task<Image?> GetByIdAsync(int id);
    Task<List<Image>> GetByProductIdAsync(int productId);
    Task<int> CreateAsync(Image image);
    Task<bool> UpdateAsync(Image image);
    Task<bool> DeleteAsync(int imageId);
}

public class ImageRepository(DatabaseContext dbContext) : IImageRepository
{
    public async Task<Image?> GetByIdAsync(int id)
    {
        const string query = "SELECT id, path, product_id FROM images WHERE id = @id";
        var parameters = new[]{ new NpgsqlParameter("@id", id) };
        var images = await dbContext.ExecuteQueryAsync(query, MapImage, parameters);
        return images.FirstOrDefault();
    }

    public async Task<List<Image>> GetByProductIdAsync(int productId)
    {
        const string query = "SELECT id, path, product_id FROM images WHERE product_id = @id";
        var parameters = new[]{ new NpgsqlParameter("@id", productId) };
        var images = await dbContext.ExecuteQueryAsync(query, MapImage, parameters);
        return images;
    }

    public async Task<int> CreateAsync(Image image)
    {
        const string query = """
                             INSERT INTO images (id, path, product_id)
                             VALUES (@id, @path, @productId)
                             RETURNING id
                             """;
        image.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        var parameters = new[]
        {
            new NpgsqlParameter("@id", image.Id),
            new NpgsqlParameter( "@path", image.Path),
            new NpgsqlParameter( "@productId", image.ProductId),
        };
        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Image image)
    {
        const string query = """
                             UPDATE images
                             SET path = @path,
                                 product_id = @productId
                             WHERE id = @id
                             """;

        var parameters = new[]
        {
            new NpgsqlParameter("@id", image.Id),
            new NpgsqlParameter("@path", image.Path),
            new NpgsqlParameter("@productId", image.ProductId),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int imageId)
    {
        const string query = "DELETE FROM images WHERE id = @id";
        var parameters = new[] { new NpgsqlParameter("@id", imageId) };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    private static Image MapImage(NpgsqlDataReader reader)
        => new Image
        {
            Id = reader.GetInt32(0),
            Path = reader.GetString(1),
            ProductId = reader.GetInt32(2)
        };
}