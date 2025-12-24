using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels.Account;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Número de teléfono inválido")]
    public string? PhoneNumber { get; set; }
    public string? TelegramUser { get; set; }
    
    public string? CurrentProfilePic { get; set; }
    public IFormFile? NewProfilePic { get; set; }
}