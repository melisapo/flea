using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.ViewModels.Auth;

namespace flea_WebProj.Services;

public interface IUserService
{
    Task<(bool success, string message)> ChangeUsernameAsync(int userId, ChangeUserViewModel model);
    Task<(bool success, string message)> UpdateProfileAsync(int userId, EditProfileViewModel model);
    Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    Task<(bool success, string message, string? newPath)> UpdateProfilePictureAsync(int userId, IFormFile newProfilePic);
    Task<EditProfileViewModel?> GetEditProfileDataAsync(int userId);
}

public class UserService(
    IUserRepository userRepository,
    IContactRepository contactRepository,
    IPasswordHasher passwordHasher,
    IFileUploadService fileUploadService) : IUserService
{
    public async Task<(bool success, string message)> ChangeUsernameAsync(int userId, ChangeUserViewModel model)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) 
                return (false, $"Usuario no encontrado");
            
            var validUser = await userRepository.UsernameExistsAsync(model.Username);
            if (!validUser) return (false, "El nombre de usuario ya está en uso");

            var updated = await userRepository.UpdateUsernameAsync(userId, model.Username);
            return updated ? 
                (true, "Usuario actualizado exitosamente") : 
                (false, "Error al actualizar usuario");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar usuario: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) 
                return (false, $"Usuario no encontrado");

            user.Name = model.Name;
            await userRepository.UpdateAsync(user);
        
            var contact = await contactRepository.GetByUserIdAsync(userId);
            if (contact == null) return (false, "Contacto no encontrado");
            
            contact.Email = model.Email;
            contact.PhoneNumber = model.PhoneNumber;
            contact.TelegramUser = model.TelegramUser;
            
            var updated = await contactRepository.UpdateAsync(contact);

            return updated ? 
                (true, "Perfil actualizado exitosamente") : 
                (false, "Error al actualizar perfil");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar perfil: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Usuario no encontrado");
            
            if (!passwordHasher.Verify(model.CurrentPassword, user.PasswordHash))
                return (false, "La contraseña actual es incorrecta");
            
            var newPasswordHash = passwordHasher.Hash(model.NewPassword);
            var updated = await userRepository.UpdatePasswordAsync(userId, newPasswordHash);

            return updated ? 
                (true, "Contraseña actualizada exitosamente") : 
                (false, "Error al actualizar contraseña");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar contraseña: {ex.Message}");
        }
    }

    public async Task<(bool success, string message, string? newPath)> UpdateProfilePictureAsync(int userId, IFormFile newProfilePic)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Usuario no encontrado", null);
            
            var (success, filePath, error) = await fileUploadService.UploadImageAsync(newProfilePic, "profiles");
            
            if (!success || filePath == null)
                return (false, error ?? "Error al subir imagen", null);
            
            if (user.ProfilePicture != "/images/default-avatar.png")
                fileUploadService.DeleteImage(user.ProfilePicture);

            var updated = await userRepository.UpdateProfilePicAsync(userId, filePath);

            return updated ? 
                (true, "Foto de perfil actualizada", filePath) : 
                (false, "Error al actualizar foto de perfil", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar foto: {ex.Message}", null);
        }
    }

    public async Task<EditProfileViewModel?> GetEditProfileDataAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var contact = await contactRepository.GetByUserIdAsync(userId);

        return new EditProfileViewModel
        {
            Name = user.Name,
            Email = contact?.Email ?? "",
            PhoneNumber = contact?.PhoneNumber,
            TelegramUser = contact?.TelegramUser,
            CurrentProfilePic = user.ProfilePicture
        };
    }
}