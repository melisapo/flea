using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(int id);
    Task<Post?> GetWithFullDetailsAsync(int id);
    Task<int> CreateAsync(Post post);
    Task<bool> UpdateAsync(Post post);
    Task<bool> DeleteAsync(int id);
    Task<List<Post>> GetRecentPostsAsync(int limit = 12);
    Task<List<Post>> GetByAuthorAsync(int authorId, int limit = 20);
    Task<Post?> GetByProductIdAsync(int productId);
    Task<List<Post>> SearchPostsAsync(string searchTerm, int limit = 20);
    Task<List<Post>> GetByCategoryAsync(int categoryId, int limit = 20);
    Task<List<Post>> GetWithFiltersAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null,
        int page = 1,
        int pageSize = 12
    );
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
    // Obtener post por ID (solo post)
    public async Task<Post?> GetByIdAsync(int id)
    {
        const string query = @"
            SELECT id, title, description, created_at, updated_at, product_id, author_id
            FROM posts
            WHERE id = @id";
        
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var posts = await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
        return posts.FirstOrDefault();
    }

    // Obtener post con producto, imágenes, categorías y autor
    public async Task<Post?> GetWithFullDetailsAsync(int id)
    {
        // 1. Obtener post
        var post = await GetByIdAsync(id);
        if (post == null) return null;

        // 2. Obtener producto
        const string productQuery = @"
            SELECT id, price, status
            FROM products
            WHERE id = @productId";
        
        var productParams = new[] { new NpgsqlParameter("@productId", post.ProductId) };
        var products = await dbContext.ExecuteQueryAsync(productQuery, MapProduct, productParams);
        post.Product = products.FirstOrDefault();

        if (post.Product != null)
        {
            // 3. Obtener categorías del producto
            const string categoriesQuery = @"
                SELECT c.id, c.name, c.slug
                FROM categories c
                INNER JOIN product_categories pc ON c.id = pc.category_id
                WHERE pc.product_id = @productId";
            
            var categoryParams = new[] { new NpgsqlParameter("@productId", post.ProductId) };
            post.Product.Categories = await dbContext.ExecuteQueryAsync(categoriesQuery, MapCategory, categoryParams);

            // 4. Obtener imágenes del producto
            const string imagesQuery = @"
                SELECT id, path, product_id
                FROM images
                WHERE product_id = @productId";
            
            var imageParams = new[] { new NpgsqlParameter("@productId", post.ProductId) };
            post.Product.Images = await dbContext.ExecuteQueryAsync(imagesQuery, MapImage, imageParams);
        }

        // 5. Obtener autor
        const string authorQuery = @"
            SELECT id, username, name, password_hash, profile_pic, created_at, updated_at
            FROM users
            WHERE id = @authorId";
        
        var authorParams = new[] { new NpgsqlParameter("@authorId", post.AuthorId) };
        var authors = await dbContext.ExecuteQueryAsync(authorQuery, MapUser, authorParams);
        post.Author = authors.FirstOrDefault();

        return post;
    }

    // Crear post
    public async Task<int> CreateAsync(Post post)
    {
        const string query = @"
            INSERT INTO posts (id, title, description, created_at, updated_at, product_id, author_id)
            VALUES (@id, @title, @description, @created_at, @updated_at, @product_id, @author_id)
            RETURNING id";
        
        post.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        post.CreatedAt = DateTime.UtcNow;
        
        var parameters = new[]
        {
            new NpgsqlParameter("@id", post.Id),
            new NpgsqlParameter("@title", post.Title),
            new NpgsqlParameter("@description", post.Description),
            new NpgsqlParameter("@created_at", post.CreatedAt),
            new NpgsqlParameter("@updated_at", (object?)post.UpdatedAt ?? DBNull.Value),
            new NpgsqlParameter("@product_id", post.ProductId),
            new NpgsqlParameter("@author_id", post.AuthorId)
        };
        
        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    // Actualizar post
    public async Task<bool> UpdateAsync(Post post)
    {
        const string query = @"
            UPDATE posts
            SET title = @title,
                description = @description,
                updated_at = @updated_at
            WHERE id = @id";
        
        post.UpdatedAt = DateTime.UtcNow;
        
        var parameters = new[]
        {
            new NpgsqlParameter("@id", post.Id),
            new NpgsqlParameter("@title", post.Title),
            new NpgsqlParameter("@description", post.Description),
            new NpgsqlParameter("@updated_at", post.UpdatedAt)
        };
        
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    // Eliminar post
    public async Task<bool> DeleteAsync(int id)
    {
        const string query = "DELETE FROM posts WHERE id = @id";
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    // Obtener posts recientes
    public async Task<List<Post>> GetRecentPostsAsync(int limit = 12)
    {
        const string query = @"
            SELECT p.id, p.title, p.description, p.created_at, p.updated_at, p.product_id, p.author_id
            FROM posts p
            ORDER BY p.created_at DESC
            LIMIT @limit";
        
        var parameters = new[] { new NpgsqlParameter("@limit", limit) };
        return await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
    }

    // Obtener posts por autor
    public async Task<List<Post>> GetByAuthorAsync(int authorId, int limit = 20)
    {
        const string query = @"
            SELECT id, title, description, created_at, updated_at, product_id, author_id
            FROM posts
            WHERE author_id = @authorId
            ORDER BY created_at DESC
            LIMIT @limit";
        
        var parameters = new[]
        {
            new NpgsqlParameter("@authorId", authorId),
            new NpgsqlParameter("@limit", limit)
        };
        
        return await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
    }

    // Obtener post por product_id
    public async Task<Post?> GetByProductIdAsync(int productId)
    {
        const string query = @"
            SELECT id, title, description, created_at, updated_at, product_id, author_id
            FROM posts
            WHERE product_id = @productId";
        
        var parameters = new[] { new NpgsqlParameter("@productId", productId) };
        var posts = await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
        return posts.FirstOrDefault();
    }

    // Buscar posts
    public async Task<List<Post>> SearchPostsAsync(string searchTerm, int limit = 20)
    {
        const string query = @"
            SELECT id, title, description, created_at, updated_at, product_id, author_id
            FROM posts
            WHERE LOWER(title) LIKE @searchTerm OR LOWER(description) LIKE @searchTerm
            ORDER BY created_at DESC
            LIMIT @limit";
        
        var parameters = new[]
        {
            new NpgsqlParameter("@searchTerm", $"%{searchTerm.ToLower()}%"),
            new NpgsqlParameter("@limit", limit)
        };
        
        return await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
    }

    // Obtener posts por categoría
    public async Task<List<Post>> GetByCategoryAsync(int categoryId, int limit = 20)
    {
        const string query = @"
            SELECT p.id, p.title, p.description, p.created_at, p.updated_at, p.product_id, p.author_id
            FROM posts p
            INNER JOIN product_categories pc ON p.product_id = pc.product_id
            WHERE pc.category_id = @categoryId
            ORDER BY p.created_at DESC
            LIMIT @limit";
        
        var parameters = new[]
        {
            new NpgsqlParameter("@categoryId", categoryId),
            new NpgsqlParameter("@limit", limit)
        };
        
        return await dbContext.ExecuteQueryAsync(query, MapPost, parameters);
    }

    // Obtener posts con filtros (el más complejo)
    public async Task<List<Post>> GetWithFiltersAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null,
        int page = 1,
        int pageSize = 12)
    {
        var query = @"
            SELECT DISTINCT p.id, p.title, p.description, p.created_at, p.updated_at, p.product_id, p.author_id
            FROM posts p
            INNER JOIN products pr ON p.product_id = pr.id";
        
        // Si hay filtro de categoría, hacer JOIN
        if (categoryId.HasValue)
        {
            query += " INNER JOIN product_categories pc ON p.product_id = pc.product_id";
        }
        
        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        
        // Filtro de búsqueda
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            conditions.Add("(LOWER(p.title) LIKE @searchTerm OR LOWER(p.description) LIKE @searchTerm)");
            parameters.Add(new NpgsqlParameter("@searchTerm", $"%{searchTerm.ToLower()}%"));
        }
        
        // Filtro de categoría
        if (categoryId.HasValue)
        {
            conditions.Add("pc.category_id = @categoryId");
            parameters.Add(new NpgsqlParameter("@categoryId", categoryId.Value));
        }
        
        // Filtro de precio mínimo
        if (minPrice.HasValue)
        {
            conditions.Add("pr.price >= @minPrice");
            parameters.Add(new NpgsqlParameter("@minPrice", minPrice.Value));
        }
        
        // Filtro de precio máximo
        if (maxPrice.HasValue)
        {
            conditions.Add("pr.price <= @maxPrice");
            parameters.Add(new NpgsqlParameter("@maxPrice", maxPrice.Value));
        }
        
        // Filtro de status
        if (!string.IsNullOrWhiteSpace(status))
        {
            conditions.Add("pr.status = @status");
            parameters.Add(new NpgsqlParameter("@status", status));
        }
        
        // Agregar WHERE si hay condiciones
        if (conditions.Any())
        {
            query += " WHERE " + string.Join(" AND ", conditions);
        }
        
        // Ordenar y paginar
        query += " ORDER BY p.created_at DESC";
        query += " LIMIT @pageSize OFFSET @offset";
        
        parameters.Add(new NpgsqlParameter("@pageSize", pageSize));
        parameters.Add(new NpgsqlParameter("@offset", (page - 1) * pageSize));
        
        return await dbContext.ExecuteQueryAsync(query, MapPost, parameters.ToArray());
    }

    // Contar total de posts con filtros
    public async Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null)
    {
        var query = @"
            SELECT COUNT(DISTINCT p.id)
            FROM posts p
            INNER JOIN products pr ON p.product_id = pr.id";
        
        if (categoryId.HasValue)
        {
            query += " INNER JOIN product_categories pc ON p.product_id = pc.product_id";
        }
        
        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            conditions.Add("(LOWER(p.title) LIKE @searchTerm OR LOWER(p.description) LIKE @searchTerm)");
            parameters.Add(new NpgsqlParameter("@searchTerm", $"%{searchTerm.ToLower()}%"));
        }
        
        if (categoryId.HasValue)
        {
            conditions.Add("pc.category_id = @categoryId");
            parameters.Add(new NpgsqlParameter("@categoryId", categoryId.Value));
        }
        
        if (minPrice.HasValue)
        {
            conditions.Add("pr.price >= @minPrice");
            parameters.Add(new NpgsqlParameter("@minPrice", minPrice.Value));
        }
        
        if (maxPrice.HasValue)
        {
            conditions.Add("pr.price <= @maxPrice");
            parameters.Add(new NpgsqlParameter("@maxPrice", maxPrice.Value));
        }
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            conditions.Add("pr.status = @status");
            parameters.Add(new NpgsqlParameter("@status", status));
        }
        
        if (conditions.Any())
        {
            query += " WHERE " + string.Join(" AND ", conditions);
        }
        
        var result = await dbContext.ExecuteScalarAsync(query, parameters.ToArray());
        return Convert.ToInt32(result);
    }

    // Métodos de mapeo
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

    private static Product MapProduct(NpgsqlDataReader reader)
        => new Product
        {
            Id = reader.GetInt32(0),
            Price = reader.GetDecimal(1),
            Status = reader.GetString(2)
        };

    private static Category MapCategory(NpgsqlDataReader reader)
        => new Category
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Slug = reader.GetString(2)
        };

    private static Image MapImage(NpgsqlDataReader reader)
        => new Image
        {
            Id = reader.GetInt32(0),
            Path = reader.GetString(1),
            ProductId = reader.GetInt32(2)
        };

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
}