using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.ViewModels.Auth;

namespace flea_WebProj.Services;

public interface IUserService
{
    Task<(bool success, string message)> ChangeUsernameAsync(int userId, ChangeUserViewModel model);
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

    public Task<(bool success, string message)> ChangeUsernameAsync(int userId, ChangeUserViewModel model)
    {
        throw new NotImplementedException();
    }

    public async Task<(bool success, string message)> UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) 
                return (false, $"Usuario no encontrado");

            user.Name = model.Name;
            await _userRepository.UpdateAsync(user);
        
            var contact = await _contactRepository.GetByUserIdAsync(userId);
            if (contact == null) return (false, "Contacto no encontrado");
            
            contact.Email = model.Email;
            contact.PhoneNumber = model.PhoneNumber;
            contact.TelegramUser = model.TelegramUser;
            
            await _contactRepository.UpdateAsync(contact);
            
            return (true, "Perfil actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar perfil: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
    {
        
    }

    public async Task<(bool success, string message, string? newPath)> UpdateProfilePictureAsync(int userId, IFormFile newProfilePic)
    {
        throw new NotImplementedException();
    }
}