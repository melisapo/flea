using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    
    [Phone(ErrorMessage = "Número de teléfono inválido")]
    public string? PhoneNumber { get; set; }

    public string? TelegramUser { get; set; }
    
    public string? City { get; set; }

    [Required(ErrorMessage = "El estado/provincia es requerido")]
    public string StateProvince { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país es requerido")]
    public string Country { get; set; } = string.Empty;
}