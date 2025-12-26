using flea_WebProj.Models.Entities;
using Npgsql;

namespace flea_WebProj.Data.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetWithDetailsAsync(int id);
    Task<int> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStatusAsync(int productId, string status);
    Task<List<Product>> GetByStatusAsync(string status, int limit = 20);
    Task<List<Product>> GetByCategoryAsync(int categoryId, int limit = 20);
    Task<List<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, int limit = 20);
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

        const string categoriesQuery = """
                    SELECT c.id, c.name, c.slug
                    FROM categories c
                    INNER JOIN product_categories pc ON c.id = pc.category_id
                    WHERE pc.product_id = @productId
            """;

        var categoryParams = new[] { new NpgsqlParameter("@productId", id) };
        product.Categories = await dbContext.ExecuteQueryAsync(
            categoriesQuery,
            MapCategory,
            categoryParams
        );

        const string imagesQuery = """
                    SELECT id, path, product_id
                    FROM images
                    WHERE product_id = @productId
            """;

        var imageParams = new[] { new NpgsqlParameter("@productId", id) };
        product.Images = await dbContext.ExecuteQueryAsync(imagesQuery, MapImage, imageParams);

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

    public async Task<bool> UpdateStatusAsync(int productId, string status)
    {
        const string query = """
            UPDATE products
            SET status = @status
            WHERE id = @id
            """;
        var parameters = new[]
        {
            new NpgsqlParameter("@id", productId),
            new NpgsqlParameter("@status", status),
        };
        var rowsAffected = await dbContext.ExecuteNonQueryAsync(query, parameters);
        return rowsAffected > 0;
    }

    public async Task<List<Product>> GetByStatusAsync(string status, int limit = 20)
    {
        const string query = """
            SELECT id, price, status 
            FROM products 
            WHERE status = @status
            ORDER BY id DESC
            LIMIT @limit
            """;
        var parameters = new[]
        {
            new NpgsqlParameter("@status", status),
            new NpgsqlParameter("@limit", limit),
        };
        var products = await dbContext.ExecuteQueryAsync(query, MapProduct, parameters);
        return products;
    }

    public async Task<List<Product>> GetByCategoryAsync(int categoryId, int limit = 20)
    {
        const string query = """
            SELECT p.id, p.price, p.status
            FROM products p
            INNER JOIN product_categories pc ON p.id = pc.product_id
            WHERE pc.category_id = @categoryId
            ORDER BY p.id DESC
            LIMIT @limit
            """;
        var parameters = new[]
        {
            new NpgsqlParameter("@categoryId", categoryId),
            new NpgsqlParameter("@limit", limit),
        };
        var products = await dbContext.ExecuteQueryAsync(query, MapProduct, parameters);
        return products;
    }

    public async Task<List<Product>> GetByPriceRangeAsync(
        decimal minPrice,
        decimal maxPrice,
        int limit = 20
    )
    {
        const string query = """
            SELECT id, price, status
            FROM products
            WHERE price BETWEEN @minPrice AND @maxPrice
            ORDER BY price
            LIMIT @limit
            """;
        var parameters = new[]
        {
            new NpgsqlParameter("@minPrice", minPrice),
            new NpgsqlParameter("@maxPrice", maxPrice),
            new NpgsqlParameter("@limit", limit),
        };
        var products = await dbContext.ExecuteQueryAsync(query, MapProduct, parameters);
        return products;
    }

    private static Product MapProduct(NpgsqlDataReader reader) =>
        new Product
        {
            Id = reader.GetInt32(0),
            Price = reader.GetDecimal(1),
            Status = reader.GetString(2),
        };

    private static Category MapCategory(NpgsqlDataReader reader) =>
        new Category
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Slug = reader.GetString(2),
        };

    private static Image MapImage(NpgsqlDataReader reader) =>
        new Image
        {
            Id = reader.GetInt32(0),
            Path = reader.GetString(1),
            ProductId = reader.GetInt32(2),
        };
}
