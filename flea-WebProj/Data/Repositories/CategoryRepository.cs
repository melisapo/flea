using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetBySlugAsync(string slug);
    Task<List<Category>> GetAllAsync();
    Task<int> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignCategoryToProductAsync(int productId, int categoryId);
    Task<bool> RemoveCategoryFromProductAsync(int productId, int categoryId);
    Task<List<Category>> GetProductCategoryAsync(int productId);
}

public class CategoryRepository(DatabaseContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(int id)
    {
        const string query = "SELECT id, name, slug FROM categories WHERE id = @id";
        var parameters = new[]{ new NpgsqlParameter("@id", id) };
        var categories = await dbContext.ExecuteQueryAsync(query, MapCategory, parameters);
        return categories.FirstOrDefault();
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        const string query = "SELECT id, name, slug FROM categories WHERE slug = @slug";
        var parameters = new[] { new NpgsqlParameter("@slug", slug) };
        var categories = await dbContext.ExecuteQueryAsync(query, MapCategory, parameters);
        return categories.FirstOrDefault();
    }

    public async Task<List<Category>> GetAllAsync()
    {
        const string query = """
                             SELECT id, name, slug 
                             FROM categories 
                             ORDER BY name
                             """;
        return await dbContext.ExecuteQueryAsync(query, MapCategory);
    }

    public async Task<int> CreateAsync(Category category)
    {
        const string query = """
                             INSERT INTO  categories (id, name, slug)
                             VALUES  (@id, @name, @slug)
                             RETURNING id
                             """;
        category.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        var parameters = new[]
        {
            new NpgsqlParameter("@id", category.Id),
            new NpgsqlParameter( "@name", category.Name),
            new NpgsqlParameter( "@slug", category.Slug),
        };
        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        const string checkQuery =
            """
            SELECT COUNT(*) FROM categories
            WHERE slug = @slug AND id != @id
            """;

        var checkParams = new[] { new NpgsqlParameter("@slug", category.Slug) };

        var exists =
            Convert.ToInt64(await dbContext.ExecuteScalarAsync(checkQuery, checkParams)) > 0;
        if (exists)
            return false;
        
        const string query = """
                             UPDATE categories
                             SET name = @name,
                                slug = @slug
                             WHERE id = @id
                             """;

        var parameters = new[]
        {
            new NpgsqlParameter("@id", category.Id),
            new NpgsqlParameter("@name", category.Name),
            new NpgsqlParameter("@slug", category.Slug)
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string query = "DELETE FROM categories WHERE id = @id";
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> AssignCategoryToProductAsync(int productId, int categoryId)
    {
        // Verificar si ya existe la relación
        const string checkQuery =
            """
            SELECT COUNT(*) FROM product_categories
            WHERE product_id = @productId AND category_id = @categoryId
            """;

        var checkParams = new[]
        {
            new NpgsqlParameter("@productId", productId),
            new NpgsqlParameter("@categoryId", categoryId),
        };

        var exists =
            Convert.ToInt64(await dbContext.ExecuteScalarAsync(checkQuery, checkParams)) > 0;
        if (exists)
            return true;
        
        const string insertQuery =
            """
            INSERT INTO product_categories (id, product_id, category_id)
            VALUES (@id, @productId, @categoryId)
            """;

        var id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);
        var insertParams = new[]
        {
            new NpgsqlParameter("@id", id),
            new NpgsqlParameter("@productId", productId),
            new NpgsqlParameter("@categoryId", categoryId),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(insertQuery, insertParams);
        return rowsAffected > 0;
    }

    public async Task<bool> RemoveCategoryFromProductAsync(int productId, int categoryId)
    {
        const string query =
            """
            DELETE FROM product_categories 
            WHERE product_id = @productId AND category_id = @categoryId
            """;

        var parameters = new[]
        {
            new NpgsqlParameter("@productId", productId),
            new NpgsqlParameter("@categoryId", categoryId),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<List<Category>> GetProductCategoryAsync(int productId)
    {
        const string query =
            """
            SELECT c.id, c.name, c.slug
            FROM categories c
            INNER JOIN product_categories pc ON c.id = pc.category_id
            WHERE pc.product_id = @productId
            """;

        var parameters = new[] { new NpgsqlParameter("@productId", productId) };
        return await dbContext.ExecuteQueryAsync(query, MapCategory, parameters);
    }
    
    private static Category MapCategory(NpgsqlDataReader reader)
        => new Category
        {
            Id = reader.GetInt32(0), 
            Name = reader.GetString(1), 
            Slug = reader.GetString(2)
        };
}