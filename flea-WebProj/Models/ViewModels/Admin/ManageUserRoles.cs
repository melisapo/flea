using System.ComponentModel.DataAnnotations;
using flea_WebProj.Models.Entities;

namespace flea_WebProj.Models.ViewModels.Admin;

public class ManageUserRoles
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    public List<Role> Roles { get; set; } = [];

    [Required(ErrorMessage = "Debe seleccionar al menos un rol")]
    public List<int> UserRolesIds { get; set; } = [];
}