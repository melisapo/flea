using flea_WebProj.Data.Repositories;
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

public class AdminService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    IProductRepository productRepository,
    IImageRepository imageRepository,
    IFileUploadService fileUploadService)
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

    public Task<Dictionary<string, int>> GetDashboardStatsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Post>> GetAllPostsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Post?> GetPostByIdAsync(int postId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePostAsync(int postId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Category>> GetAllCategoriesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Category?> GetCategoryByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Category> CreateCategoryAsync(string name, string slug)
    {
        throw new NotImplementedException();
    }

    public Task<Category> UpdateCategoryAsync(int id, string name, string slug)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteCategoryAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Role>> GetAllRolesAsync()
    {
        throw new NotImplementedException();
    }
}