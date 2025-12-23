using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El password es requerido")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
