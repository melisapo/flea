namespace flea_WebProj.Models.ViewModels.Admin;

public class ManageUserRolesViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // Paginación
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalUsers { get; set; }
    public int TotalPages { get; set; }
    
    // Roles actuales del usuario
    public List<int> CurrentRoleIds { get; set; } = [];
    
    // Todos los roles disponibles
    public List<RoleItem> AvailableRoles { get; set; } = [];
}

public class RoleItem
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}