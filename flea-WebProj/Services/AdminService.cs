using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels.Admin;

namespace flea_WebProj.Services;

public interface IAdminService
{
    // User Management
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
    Task<List<Role>> GetUserRolesAsync(int userId);
    Task<DashboardViewModel> GetDashboardStatsAsync();
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<bool> DeleteUserAsync(int userId);
        
    // Post Management
    Task<List<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(int postId);
    Task<bool> DeletePostAsync(int postId);
        
    // Category Management
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(CreateCategoryViewModel model);
    Task<Category> UpdateCategoryAsync(EditCategoryViewModel model);
    Task<bool> DeleteCategoryAsync(int id);
    
    //Role Management
    Task<List<Role>> GetAllRolesAsync();
}

public class AdminService(
    IUserService userService,
    ICategoryService categoryService,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPostRepository postRepository,
    IProductRepository productRepository,
    ICategoryRepository categoryRepository)
    : IAdminService
{
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await userRepository.GetAllAsync();
    }

    public Task<User?> GetUserByIdAsync(int userId)
    {
        return userRepository.GetByIdAsync(userId);
    }

    public async Task<List<Role>> GetUserRolesAsync(int userId)
    {
        return await roleRepository.GetUserRolesAsync(userId);
    }

    public async Task<DashboardViewModel> GetDashboardStatsAsync()
    {
         // Estadísticas generales
            var totalUsers = await userRepository.GetAllAsync();
            var totalPosts = await postRepository.GetAllAsync();
            var totalCategories = await categoryRepository.GetAllAsync();
            
            var activePosts = await productRepository.GetByStatusAsync("Available");
            var soldPosts = await productRepository.GetByStatusAsync("Sold");

            // Fechas para filtrar actividad reciente
            var today = DateTime.UtcNow.Date;
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            // Usuarios recientes
            var allUsers = await userRepository.GetAllAsync();
            var newUsersThisWeek = allUsers.Count(u => u.CreatedAt >= oneWeekAgo);

            // Posts recientes
            var newPostsThisWeek = totalPosts.Count(p => p.CreatedAt >= oneWeekAgo);
            var newPostsToday = totalPosts.Count(p => p.CreatedAt >= today);

            // Actividad reciente (últimos 10 items)

            // Agregar usuarios recientes
            var recentUsers = allUsers
                .Where(u => u.CreatedAt >= oneWeekAgo)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5);

            var recentActivity = recentUsers.Select(user => new RecentActivityItem
                {
                    Type = "user",
                    Description = $"Nuevo usuario: {user.Username}",
                    Timestamp = user.CreatedAt,
                    Icon = "bi-person-plus",
                    Color = "text-success"
                })
                .ToList();

            // Agregar posts recientes
            var recentPosts = totalPosts
                .Where(p => p.CreatedAt >= oneWeekAgo)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5);
            
            foreach (var post in recentPosts)
            {
                var author = await userRepository.GetByIdAsync(post.AuthorId);
                recentActivity.Add(new RecentActivityItem
                {
                    Type = "post",
                    Description = $"Nueva publicación: {post.Title} por {author?.Username ?? "Usuario"}",
                    Timestamp = post.CreatedAt,
                    Icon = "bi-file-post",
                    Color = "text-primary"
                });
            }

            // Ordenar por timestamp y tomar los 10 más recientes
            recentActivity = recentActivity
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            // Top categorías (las 4 con más posts)
            var topCategoriesList = await categoryService.GetTrendingCategoriesAsync(4);
            var topCategories = new List<CategoryStatsItem>();

            if (topCategoriesList != null && topCategoriesList.Count != 0)
            {
                foreach (var category in topCategoriesList)
                {
                    var products = await productRepository.GetByCategoryIdAsync(category.Id);
                    topCategories.Add(new CategoryStatsItem
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        PostCount = products.Count
                    });
                }
            }
            
            return new DashboardViewModel
            {
                TotalUsers = totalUsers.Count,
                TotalPosts = totalPosts.Count,
                TotalCategories = totalCategories.Count,
                ActivePosts = activePosts.Count,
                SoldPosts = soldPosts.Count,
                NewUsersThisWeek = newUsersThisWeek,
                NewPostsThisWeek = newPostsThisWeek,
                NewPostsToday = newPostsToday,
                RecentActivity = recentActivity,
                TopCategories = topCategories
            };
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        return await roleRepository.AssignRoleToUserAsync(userId, roleId);
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        return await roleRepository.RemoveRoleFromUserAsync(userId, roleId);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var userDeleted = await userService.DeleteUserAsync(userId);
        return userDeleted.success;
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        return await postRepository.GetAllAsync();
    }

    public async Task<Post?> GetPostByIdAsync(int postId)
    {
        return await postRepository.GetByIdAsync(postId);
    }

    public async Task<bool> DeletePostAsync(int postId)
    {
        return await postRepository.DeleteAsync(postId);
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await categoryRepository.GetAllAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await categoryRepository.GetByIdAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(CreateCategoryViewModel model)
    {
        var category = new Category
        {
            Name = model.Name,
            Slug = model.Slug
        };
        var id =  await categoryRepository.CreateAsync(category);
        
        return await categoryRepository.GetByIdAsync(id) ?? new Category();
    }

    public async Task<Category> UpdateCategoryAsync(EditCategoryViewModel model)
    {
        var category = new Category
        {
            Id = model.CategoryId,
            Name = model.Name,
            Slug = model.Slug
        };
        await categoryRepository.UpdateAsync(category);
        return await categoryRepository.GetByIdAsync(model.CategoryId) ?? category;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await categoryRepository.DeleteAsync(id);
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await roleRepository.GetAllAsync();
    }
}