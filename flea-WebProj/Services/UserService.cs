using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.ViewModels.Auth;

namespace flea_WebProj.Services;

public interface IUserService
{
    Task<(bool success, string message)> UpdateProfileAsync(int userId, EditProfileViewModel model);
    Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    Task<(bool success, string message, string? newPath)> UpdateProfilePictureAsync(int userId, IFormFile newProfilePic);
    Task<(bool success, string messaeg)> DeleteUserAsync(int userId);
    Task<EditProfileViewModel?> GetEditProfileDataAsync(int userId);
}

public class UserService(
    IAddressRepository addressRepository,
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    IContactRepository contactRepository,
    IPasswordHasher passwordHasher,
    IPostRepository postRepository,
    IFileUploadService fileUploadService) : IUserService
{

    public async Task<(bool success, string message)> UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) 
                return (false, $"Usuario no encontrado");

            user.Name = model.Name;
            await userRepository.UpdateAsync(user);
            
            var existUser = await userRepository.UsernameExistsAsync(model.Username, userId);
            if (existUser) return (false, "El nombre de usuario ya está en uso");

            await userRepository.UpdateUsernameAsync(userId, model.Username);
        
            var contact = await contactRepository.GetByUserIdAsync(userId);
            if (contact == null) return (false, "Contacto no encontrado");
            
            contact.Email = model.Email;
            contact.PhoneNumber = model.PhoneNumber;
            contact.TelegramUser = model.TelegramUser?.Trim().TrimStart('@');
            
            await contactRepository.UpdateAsync(contact);
            
            var address = await addressRepository.GetByUserIdAsync(userId);
            if (address == null) return (false, "Direccion no encontrada");

            address.City = model.City;
            address.StateProvince = model.StateProvince;
            address.Country = model.Country;
            
            await addressRepository.UpdateAsync(address);
            
            return (true, "Perfil actualizado exitosamente");
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
            await userRepository.UpdatePasswordAsync(userId, newPasswordHash);

            return (true, "Contraseña actualizada exitosamente");
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

            await userRepository.UpdateProfilePicAsync(userId, filePath);

            return (true, "Foto de perfil actualizada", filePath);
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar foto: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string messaeg)> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) return (true, "Usuario eliminado");
        
            var userRoles = await roleRepository.GetUserRolesAsync(userId);
            foreach (var role in userRoles)
            {
                await roleRepository.RemoveRoleFromUserAsync(userId, role.Id);
            }
        
            var address = await addressRepository.GetByUserIdAsync(userId);
            if (address != null) await addressRepository.DeleteAsync(address.Id);

            var contact = await contactRepository.GetByUserIdAsync(userId);
            if (contact != null) await contactRepository.DeleteAsync(contact.Id);

            var posts = await postRepository.GetByAuthorAsync(userId);
            if (posts != null)
                foreach (var post in posts)
                {
                    await postRepository.DeleteAsync(post.Id);
                }

            await userRepository.DeleteAsync(userId);

            return (true, "Usuario eliminado");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar usuario: {ex.Message}");
        }
    }

    public async Task<EditProfileViewModel?> GetEditProfileDataAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var contact = await contactRepository.GetByUserIdAsync(userId);
        var address = await addressRepository.GetByUserIdAsync(userId);

        return new EditProfileViewModel
        {
            Name = user.Name,
            Username = user.Username,
            Email = contact?.Email ?? "",
            PhoneNumber = contact?.PhoneNumber,
            TelegramUser = contact?.TelegramUser,
            CurrentProfilePic = user.ProfilePicture,
            City = address?.City,
            StateProvince = address?.StateProvince ?? "",
            Country = address?.Country ?? ""
        };
    }
}