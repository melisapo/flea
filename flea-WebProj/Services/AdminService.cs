using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels.Admin;

namespace flea_WebProj.Services;

public interface IAdminService
{
    // User Management
    Task<(List<User> users, int total)> GetAllUsersAsync(int page, int pageSize);
    Task<User> GetUserByIdAsync(int userId);
    Task<List<Role>> GetUserRolesAsync(int userId);
    Task<DashboardViewModel> GetDashboardStatsAsync();
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<bool> DeleteUserAsync(int userId);
        
    // Post Management
    Task<(List<Post> posts, int total)> GetAllPostsAsync(int page, int pageSize);
    Task<Post?> GetPostByIdAsync(int postId);
    Task<bool> DeletePostAsync(int postId);
    
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
    ICategoryRepository categoryRepository,
    IContactRepository contactRepository,
    IAddressRepository addressRepository)
    : IAdminService
{
    public async Task<(List<User> users, int total)> GetAllUsersAsync(int page, int pageSize)
    {
        var allUsers = await userRepository.GetAllAsync(1, int.MaxValue);
        var users =  await userRepository.GetAllAsync(page, pageSize);

        return (users, allUsers.Count);
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        var user =  await userRepository.GetByIdAsync(userId);
        if (user == null) return new User();
        
        user.Contact = await contactRepository.GetByUserIdAsync(user.Id);
        user.Address = await addressRepository.GetByUserIdAsync(user.Id);
        user.Posts = await postRepository.GetByAuthorAsync(user.Id);
        user.Roles = await roleRepository.GetUserRolesAsync(user.Id);

        return user;
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
                    Icon = "person_add",
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
                    Icon = "post_add",
                    Color = "text-primary"
                });
            }

            // Ordenar por timestamp y tomar los 10 más recientes
            recentActivity = recentActivity
                .OrderByDescending(a => a.Timestamp)
                .Take(7)
                .ToList();

            // Top categorías (las 4 con más posts)
            var topCategories = await categoryService.GetTrendingCategoriesAsync(4) ?? [];
            
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

    public async Task<(List<Post> posts, int total)> GetAllPostsAsync(int page, int pageSize)
    {
        // Total sin paginar (para TotalPages)
        var allPosts = await postRepository.GetWithFiltersAsync(
            page: 1,
            pageSize: int.MaxValue
        );

        var pagedPosts = await postRepository.GetWithFiltersAsync(
            page: page,
            pageSize: pageSize
        );

        return (pagedPosts, allPosts.Count);
    }

    public async Task<Post?> GetPostByIdAsync(int postId)
    {
        return await postRepository.GetByIdAsync(postId);
    }

    public async Task<bool> DeletePostAsync(int postId)
    {
        return await postRepository.DeleteAsync(postId);
    }
    
    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await roleRepository.GetAllAsync();
    }
}