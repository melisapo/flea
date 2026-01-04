using flea_WebProj.Models.Entities;

namespace flea_WebProj.Services;

public interface IAdminService
{
    // User Management
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
    Task<List<Role>> GetUserRolesAsync(int userId);
    Task<Dictionary<string, int>> GetDashboardStatsAsync();
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
    Task<Category> CreateCategoryAsync(string name, string slug);
    Task<Category> UpdateCategoryAsync(int id, string name, string slug);
    Task<bool> DeleteCategoryAsync(int id);
    
    //Role Management
    Task<List<Role>> GetAllRolesAsync();
}

public class AdminService
{
    
}