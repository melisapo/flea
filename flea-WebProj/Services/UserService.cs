using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.ViewModels.Auth;

namespace flea_WebProj.Services;

public interface IUserService
{
    Task<(bool success, string message)> UpdateProfileAsync(int userId, EditProfileViewModel model);
    Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    Task<(bool success, string message, string? newPath)> UpdateProfilePictureAsync(int userId, IFormFile newProfilePic);
}

public class UserService(
    IUserRepository userRepository,
    IContactRepository contactRepository,
    IPasswordHasher passwordHasher,
    IFileUploadService fileUploadService) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IContactRepository _contactRepository = contactRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IFileUploadService _fileUploadService = fileUploadService;
    
    public Task<(bool success, string message)> UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
    {
        throw new NotImplementedException();
    }

    public Task<(bool success, string message, string? newPath)> UpdateProfilePictureAsync(int userId, IFormFile newProfilePic)
    {
        throw new NotImplementedException();
    }
}