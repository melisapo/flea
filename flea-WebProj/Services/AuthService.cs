using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.Enums;
using flea_WebProj.Models.ViewModels.Auth;

namespace flea_WebProj.Services
{
    public interface IAuthService
    {
        Task<(bool success, string message, User? user)> RegisterAsync(RegisterViewModel model);
        Task<(bool success, string message, User? user)> LoginAsync(LoginViewModel model);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserWithRolesAsync(int userId);
        Task<User?> GetFullUserProfileAsync(int userId);
    }

    public class AuthService(
        IUserRepository userRepository,
        IContactRepository contactRepository,
        IAddressRepository addressRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
        : IAuthService
    {
        // Registrar usuario
        public async Task<(bool success, string message, User? user)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                if (await userRepository.UsernameExistsAsync(model.Username))
                    return (false, "El nombre de usuario ya está en uso", null);
                
                if (await userRepository.EmailExistsAsync(model.Email))
                    return (false, "El email ya está registrado", null);
                
                var user = new User
                {
                    Username = model.Username,
                    Name = model.Name,
                    PasswordHash = passwordHasher.Hash(model.Password),
                    ProfilePicture = "/images/default-avatar.png",
                    CreatedAt = DateTime.UtcNow
                };

                var userId = await userRepository.CreateAsync(user);
                user.Id = userId;
                
                var contact = new Contact
                {
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    TelegramUser = model.TelegramUser,
                    UserId = userId
                };

                await contactRepository.CreateAsync(contact);
                
                if (!string.IsNullOrWhiteSpace(model.StateProvince) && !string.IsNullOrWhiteSpace(model.Country))
                {
                    var address = new Address
                    {
                        City = model.City,
                        StateProvince = model.StateProvince,
                        Country = model.Country,
                        UserId = userId
                    };

                    await addressRepository.CreateAsync(address);
                }
                
                var userRole = await roleRepository.GetByNameAsync(nameof(RoleType.User));
                if (userRole != null)
                    await roleRepository.AssignRoleToUserAsync(userId, userRole.Id);
                
                var registeredUser = await userRepository.GetWithRolesAsync(userId);

                return (true, "Usuario registrado exitosamente", registeredUser);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar usuario: {ex.Message}", null);
            }
        }

        // Login de usuario
        public async Task<(bool success, string message, User? user)> LoginAsync(LoginViewModel model)
        {
            try
            {
                var user = await userRepository.GetByUsernameAsync(model.Username);

                if (user == null || !passwordHasher.Verify(model.Password, user.PasswordHash))
                    return (false, "Usuario o contraseña incorrectos", null);
                
                var userWithRoles = await userRepository.GetWithRolesAsync(user.Id);
                return (true, "Login exitoso", userWithRoles);
            }
            catch (Exception ex)
            {
                return (false, $"Error al iniciar sesión: {ex.Message}", null);
            }
        }

        // Obtener usuario por ID
        public async Task<User?> GetUserByIdAsync(int userId)
            => await userRepository.GetByIdAsync(userId);

        // Obtener usuario con roles
        public async Task<User?> GetUserWithRolesAsync(int userId)
            => await userRepository.GetWithRolesAsync(userId);
        
        //Obtener usuario con direcciones y contacto
        public async Task<User?> GetFullUserProfileAsync(int userId)
            => await userRepository.GetFullUserAsync(userId);
    }
}