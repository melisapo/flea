using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels.Auth;

public class ChangeUserViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
    public string Username { get; set; } = string.Empty;
}