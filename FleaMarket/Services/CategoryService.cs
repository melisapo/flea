using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels.Admin;

namespace flea_WebProj.Services;

public interface ICategoryService
{
    public Task<List<Category>?> GetAllCategoriesAsync();
    public Task<Category?> GetCategoryByIdAsync(int categoryId);
    Task<List<CategoryStatsItem>?> GetTrendingCategoriesAsync(int limit);
    Task<List<Category>> GetByProductIdAsync(int productId);
    Task<(bool success, string message)> CreateCategoryAsync(CategoryFormViewModel model);
    Task<(bool success, string message)> UpdateCategoryAsync(CategoryFormViewModel model);
    public Task<(bool success, string message)> DeleteCategoryAsync(int categoryId);
}

public class CategoryService(
    ICategoryRepository categoryRepository,
    IPostRepository postRepository) : ICategoryService
{
    public async Task<List<Category>?> GetAllCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return categories;
    }

    public Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        var category = categoryRepository.GetByIdAsync(categoryId);
        return category;
    }

    public async Task<List<CategoryStatsItem>?> GetTrendingCategoriesAsync(int limit = 4)
    {
        var categories = await categoryRepository.GetTrendingCategoriesAsync(limit);
        return categories ?? [];
    }

    public async Task<List<Category>> GetByProductIdAsync(int productId)
    {
        var categoriesIds = await categoryRepository.GetProductCategoriesIdsAsync(productId);
        List<Category> categories = [];
        foreach (var id in categoriesIds)
        {
            var category = await categoryRepository.GetByIdAsync(id) ?? new Category();
            categories.Add(category);
        }
        return categories;
    }

    public async Task<(bool success, string message)> CreateCategoryAsync(CategoryFormViewModel model)
    {
        var exists = await categoryRepository.GetBySlugAsync(model.Slug);
        if (exists != null)
            return (false, "Ya existe una categoría con ese slug");

        var category = new Category
        {
            Name = model.Name,
            Slug = model.Slug
        };

        await categoryRepository.CreateAsync(category);

        return (true, "Categoría creada correctamente");
    }
    public async Task<(bool success, string message)> UpdateCategoryAsync(CategoryFormViewModel model)
    {
        if (model.CategoryId is null)
            return (false, "ID de categoría inválido");

        var category = await categoryRepository.GetByIdAsync(model.CategoryId.Value);
        if (category == null)
            return (false, "Categoría no encontrada");

        var slugInUse = await categoryRepository.ExistSlugAsync(model.Slug, model.CategoryId.Value);

        if (slugInUse)
            return (false, "Ya existe otra categoría con ese slug");

        category.Name = model.Name;
        category.Slug = model.Slug;

        var updated = await categoryRepository.UpdateAsync(category);

        return updated
            ? (true, "Categoría actualizada correctamente")
            : (false, "Error al actualizar categoría");
    }


    public async Task<(bool success, string message)> DeleteCategoryAsync(int categoryId)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            return (false, "La categoría no existe");

        var posts = await postRepository.GetByCategoryAsync(categoryId);

        if (posts is { Count: > 0 })
        {
            var otherCategory = await categoryRepository.GetBySlugAsync("otros");

            foreach (var post in posts)
            {
                await categoryRepository.RemoveCategoryFromProductAsync(post.ProductId, categoryId);

                if (otherCategory != null)
                {
                    await categoryRepository.AssignCategoryToProductAsync(
                        post.ProductId,
                        otherCategory.Id
                    );
                }
            }
        }

        var deleted = await categoryRepository.DeleteAsync(categoryId);

        return deleted
            ? (true, "Categoría eliminada correctamente")
            : (false, "Error al eliminar categoría");
    }
}