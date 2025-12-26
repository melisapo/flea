using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(int id);
    Task<Post?> GetByProductIdAsync(int productId);
    Task<Post?> GetWithFullDetailsAsync(int id);
    Task<int> CreateAsync(Post post);
    Task<bool> UpdateAsync(Post post);
    Task<bool> DeleteAsync(int id);
    
    // Obtener posts recientes (para home)
    Task<List<Post>> GetRecentPostsAsync(int limit = 12);
    
    // Obtener posts por autor (mis publicaciones)
    Task<List<Post>> GetByAuthorAsync(int authorId, int limit = 20);
    
    // Buscar posts (título o descripción)
    Task<List<Post>> SearchPostsAsync(string searchTerm, int limit = 20);
    
    // Obtener posts por categoría
    Task<List<Post>> GetByCategoryAsync(int categoryId, int limit = 20);
    
    // Obtener posts con filtros (búsqueda avanzada)
    Task<List<Post>> GetWithFiltersAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null,
        int page = 1,
        int pageSize = 12
    );
    
    // Contar total de posts (para paginación)
    Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null
    );
}

public class PostRepository(DatabaseContext dbContext) : IPostRepository
{
    public async Task<Post?> GetByIdAsync(int id)
    {
        const string query = """
                             SELECT id, title, description, created_at, updated_at, product_id, author_id
                             FROM posts
                             WHERE id = @id
                             """;
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var products = await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
        return products.FirstOrDefault();
    }

    public async Task<Post?> GetByProductIdAsync(int productId)
    {
        const string query = """
                             SELECT id, title, description, created_at, updated_at, product_id, author_id
                             FROM posts
                             WHERE product_id = @productId
                             """;
        var parameters = new[] { new NpgsqlParameter("@productId", productId) };
        var products = await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
        return products.FirstOrDefault();
    }

    public Task<Post?> GetWithFullDetailsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CreateAsync(Post post)
    {
        const string query = """
                             INSERT INTO posts (id, title, description, created_at, updated_at, product_id, author_id)
                             VALUES (@id, @title, @description, CURRENT_TIMESTAMP, @updatedAt, @productId, @authorId)
                             RETURNING id
                             """;
        
        post.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        
        var parameters = new[]
        {
            new NpgsqlParameter("@id", post.Id),
            new NpgsqlParameter("@title", post.Title),
            new NpgsqlParameter("@description", post.Description),
            new NpgsqlParameter("@createdAt", post.CreatedAt),
            new NpgsqlParameter("@updatedAt", (object?)post.UpdatedAt ?? DBNull.Value),
            new NpgsqlParameter("@productId", post.ProductId),
            new NpgsqlParameter("@authorId", post.AuthorId),
        };
        
        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Post post)
    {
        const string query = """
                             UPDATE "posts"
                             SET 
                                 "title" = @title,
                                 "description" = @description,
                                 "product_id" = @productId,
                                 "updated_at" = CURRENT_TIMESTAMP
                             WHERE 
                                 "id" = @id 
                                 AND "author_id" = @authorId;
                             """;
        var parameters = new[]
        {
            new NpgsqlParameter("@id", post.Id),
            new NpgsqlParameter("@title", post.Title),
            new NpgsqlParameter("@description", post.Description),
            new NpgsqlParameter("@productId", post.ProductId),
            new NpgsqlParameter("@authorId", post.AuthorId),
        };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string query = "DELETE FROM posts WHERE id = @id";
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public Task<List<Post>> GetRecentPostsAsync(int limit = 12)
    {
        throw new NotImplementedException();
    }

    public Task<List<Post>> GetByAuthorAsync(int authorId, int limit = 20)
    {
        throw new NotImplementedException();
    }

    public Task<List<Post>> SearchPostsAsync(string searchTerm, int limit = 20)
    {
        throw new NotImplementedException();
    }

    public Task<List<Post>> GetByCategoryAsync(int categoryId, int limit = 20)
    {
        throw new NotImplementedException();
    }

    public Task<List<Post>> GetWithFiltersAsync(string? searchTerm = null, int? categoryId = null, decimal? minPrice = null,
        decimal? maxPrice = null, string? status = null, int page = 1, int pageSize = 12)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetTotalCountAsync(string? searchTerm = null, int? categoryId = null, decimal? minPrice = null,
        decimal? maxPrice = null, string? status = null)
    {
        throw new NotImplementedException();
    }

    private static Post MapPost(NpgsqlDataReader reader)
        => new()
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            CreatedAt = reader.GetDateTime(3),
            UpdatedAt = reader.GetDateTime(4),
            ProductId = reader.GetInt32(5),
            AuthorId = reader.GetInt32(6),
        };
}