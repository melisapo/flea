using flea_WebProj.Services;
using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.Enums;
using flea_WebProj.Models.ViewModels.Auth;
using Moq;
using Xunit;

namespace flea_WebProj.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IContactRepository> _contactRepo = new();
        private readonly Mock<IAddressRepository> _addressRepo = new();
        private readonly Mock<IRoleRepository> _roleRepo = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();

        private AuthService CreateService()
        {
            return new AuthService(
                _userRepo.Object,
                _contactRepo.Object,
                _addressRepo.Object,
                _roleRepo.Object,
                _passwordHasher.Object
            );
        }

        // ---------------- REGISTRO ----------------

        [Fact]
        public async Task RegisterAsync_ShouldFail_WhenUsernameExists()
        {
            _userRepo.Setup(r => r.UsernameExistsAsync("mel", 0))
                     .ReturnsAsync(true);

            var service = CreateService();

            var model = new RegisterViewModel
            {
                Username = "mel",
                Email = "mel@mail.com",
                Password = "123"
            };

            var result = await service.RegisterAsync(model);

            Assert.False(result.success);
            Assert.Equal("El nombre de usuario ya está en uso", result.message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenDataIsValid()
        {
            _userRepo.Setup(r => r.UsernameExistsAsync(It.IsAny<string>(), 0))
                     .ReturnsAsync(false);
            _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
            _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync(1);

            _roleRepo.Setup(r => r.GetByNameAsync(nameof(RoleType.User)))
                     .ReturnsAsync(new Role { Id = 1 });

            _passwordHasher.Setup(h => h.Hash(It.IsAny<string>()))
                           .Returns("hashed");

            var service = CreateService();

            var model = new RegisterViewModel
            {
                Username = "mel",
                Name = "Melissa",
                Email = "mel@mail.com",
                Password = "123"
            };

            var result = await service.RegisterAsync(model);

            Assert.True(result.success);
            Assert.Equal("Usuario registrado exitosamente", result.message);
        }

        // ---------------- LOGIN ----------------

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenUserNotFound()
        {
            _userRepo.Setup(r => r.GetByUsernameAsync("mel"))
                     .ReturnsAsync((User?)null);

            var service = CreateService();

            var result = await service.LoginAsync(new LoginViewModel
            {
                Username = "mel",
                Password = "123"
            });

            Assert.False(result.success);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenPasswordIsInvalid()
        {
            var user = new User { Id = 1, PasswordHash = "hash" };

            _userRepo.Setup(r => r.GetByUsernameAsync("mel"))
                     .ReturnsAsync(user);
            _passwordHasher.Setup(h => h.Verify("wrong", "hash"))
                           .Returns(false);

            var service = CreateService();

            var result = await service.LoginAsync(new LoginViewModel
            {
                Username = "mel",
                Password = "wrong"
            });

            Assert.False(result.success);
        }

        [Fact]
        public async Task LoginAsync_ShouldSucceed_WhenCredentialsAreValid()
        {
            var user = new User { Id = 1, PasswordHash = "hash" };

            _userRepo.Setup(r => r.GetByUsernameAsync("mel"))
                     .ReturnsAsync(user);
            _userRepo.Setup(r => r.GetWithRolesAsync(1))
                     .ReturnsAsync(user);

            _passwordHasher.Setup(h => h.Verify("123", "hash"))
                           .Returns(true);

            var service = CreateService();

            var result = await service.LoginAsync(new LoginViewModel
            {
                Username = "mel",
                Password = "123"
            });

            Assert.True(result.success);
            Assert.Equal("Login exitoso", result.message);
        }
    }
}
