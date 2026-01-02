using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels.Admin;

namespace flea_WebProj.Services;

public interface ICategoryService
{
    public Task<List<Category>?> GetAllCategoriesAsync();
    public Task<Category?> GetCategoryByIdAsync(int categoryId);
    public Task<(bool success, string message)> CreateCategoryAsync(ManageCategoriesViewModel model);
    public Task<(bool success, string message)> UpdateCategoryAsync(ManageCategoriesViewModel model);
    public Task<(bool success, string message)> DeleteCategoryAsync(int categoryId);
}

public class CategoryService(
    ICategoryRepository categoryRepository,
    IPostRepository postRepository) : ICategoryService
{
    public async Task<List<Category>?> GetAllCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return categories ?? [];
    }

    public Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        var category = categoryRepository.GetByIdAsync(categoryId);
        return category;
    }

    public async Task<(bool success, string message)> CreateCategoryAsync(ManageCategoriesViewModel model)
    {
        var category = new Category
        {
            Name = model.Name,
            Slug = model.Slug,
        };
        
        var categoryId = await categoryRepository.CreateAsync(category);
        return (true, "Categoria creado correctamente, id: " + categoryId);
    }

    public async Task<(bool success, string message)> UpdateCategoryAsync(ManageCategoriesViewModel model)
    {
        var category = new Category
        {
            Name = model.Name,
            Slug = model.Slug,
        };
        
        var updated = await categoryRepository.UpdateAsync(category);
        return !updated ? (false, "Error al actualizar categoria") : (true, "Categoria actualizada correctamente");
    }


    public async Task<(bool success, string message)> DeleteCategoryAsync(int categoryId)
    {
        var posts = await postRepository.GetByCategoryAsync(categoryId);
        if (posts != null && posts.Count != 0)
        {
            var otherCategory = await categoryRepository.GetBySlugAsync("otros");
            foreach (var post in posts)
            {
                await categoryRepository
                    .RemoveCategoryFromProductAsync(post.ProductId, categoryId);
                if (otherCategory != null)
                    await categoryRepository.AssignCategoryToProductAsync(post.ProductId, otherCategory.Id);
            }
        }
        
        var deleted = await categoryRepository.DeleteAsync(categoryId);
        
        return !deleted? (false, "Error al eliminar categoria") : (true, "Categoria eliminada");
    }
}