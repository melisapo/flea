using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetByStatusAsync(string status);
    Task<List<Product>> GetByCategoryIdAsync(int categoryId);
    Task<Product?> GetByPostIdAsync (int postId);
    Task<Product?> GetWithDetailsAsync(int id);
    Task<int> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<List<Category>> GetProductCategoriesAsync(int productId);
    Task<List<Image>> GetProductImagesAsync(int productId);
}

public class ProductRepository(DatabaseContext dbContext) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(int id)
    {
        const string query = """
            SELECT id, price, status 
            FROM products 
            WHERE id = @id
            """;
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var products = await dbContext.ExecuteQueryAsync(query, MapProduct, parameters);
        return products.FirstOrDefault();
    }

    public async Task<List<Product>> GetByStatusAsync(string status)
    {
        const string productQuery = """
                                            SELECT id, price, status 
                                            FROM products 
                                            WHERE status = @status
                                    """;

        var productParams = new[] { new NpgsqlParameter("@status", status) };
        return await dbContext.ExecuteQueryAsync(productQuery, MapProduct, productParams);
    }

    public async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
    {
        const string query = """
                             SELECT p.id, p.price, p.status
                             FROM products p 
                             INNER JOIN product_categories pc on pc.product_id = p.id
                             WHERE pc.category_id = @categoryId
                             """;
        var productParams = new[] { new NpgsqlParameter("@categoryId", categoryId) };
        return await dbContext.ExecuteQueryAsync(query, MapProduct, productParams);
    }

    public async Task<Product?> GetByPostIdAsync(int postId)
    {
        const string query = """
                             SELECT p.id, p.price, p.status
                             FROM products p 
                             INNER JOIN posts po ON po.product_id = p.id
                             WHERE po.id = @postId
                             """;
        var parameters = new[] { new NpgsqlParameter("@postId", postId) };
        var products = await dbContext.ExecuteQueryAsync(query, MapProduct, parameters);
        return products.FirstOrDefault();
    }

    public async Task<Product?> GetWithDetailsAsync(int id)
    {
        const string productQuery = """
                    SELECT id, price, status 
                    FROM products 
                    WHERE id = @id
            """;

        var productParams = new[] { new NpgsqlParameter("@id", id) };
        var products = await dbContext.ExecuteQueryAsync(productQuery, MapProduct, productParams);
        var product = products.FirstOrDefault();

        if (product == null)
            return null;

        product.Categories = await GetProductCategoriesAsync(id);
        product.Images = await GetProductImagesAsync(id);

        return product;
    }

    public async Task<int> CreateAsync(Product product)
    {
        const string query = """
            INSERT INTO products ( id, price, status)
            VALUES (@id, @price, @status)
            RETURNING id;
            """;
        product.Id = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond % int.MaxValue);

        var parameters = new[]
        {
            new NpgsqlParameter("@id", product.Id),
            new NpgsqlParameter("@price", product.Price),
            new NpgsqlParameter("@status", product.Status),
        };
        var result = await dbContext.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        const string query = """
            UPDATE products
            SET price = @price, status = @status
            WHERE id = @id
            """;
        var parameters = new[]
        {
            new NpgsqlParameter("@id", product.Id),
            new NpgsqlParameter("@price", product.Price),
            new NpgsqlParameter("@status", product.Status),
        };

        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string query = "DELETE FROM products WHERE id = @id";
        var parameters = new[] { new NpgsqlParameter("@id", id) };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<List<Category>> GetProductCategoriesAsync(int productId)
    {
        const string categoriesQuery = """
                                       SELECT c.id, c.name, c.slug
                                       FROM categories c
                                       INNER JOIN product_categories pc ON c.id = pc.category_id
                                       WHERE pc.product_id = @productId
                                       """;
            
        var categoryParams = new[] { new NpgsqlParameter("@productId", productId) };
        var categories = await dbContext.ExecuteQueryAsync(categoriesQuery, MapCategory, categoryParams);
        return categories;
    }

    public async Task<List<Image>> GetProductImagesAsync(int productId)
    {
        const string imagesQuery = """
                                   SELECT id, path, product_id
                                   FROM images
                                   WHERE product_id = @productId
                                   """;
            
        var imageParams = new[] { new NpgsqlParameter("@productId", productId) };
        var images = await dbContext.ExecuteQueryAsync(imagesQuery, MapImage, imageParams);
        return images;
    }
    
    private static Product MapProduct(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetInt32(0),
            Price = reader.GetDecimal(1),
            Status = reader.GetString(2),
        };

    private static Category MapCategory(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Slug = reader.GetString(2),
        };

    private static Image MapImage(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetInt32(0),
            Path = reader.GetString(1),
            ProductId = reader.GetInt32(2),
        };
}
