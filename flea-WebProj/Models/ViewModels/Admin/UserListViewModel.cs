namespace flea_WebProj.Models.ViewModels.Admin;

public class UserListViewModel
{
    public List<UserListItem> Users { get; set; } = [];
    
    // Filtros
    public string? SearchTerm { get; set; }
    public int? RoleFilter { get; set; }
    
    // Paginación
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalUsers { get; set; }
    public int TotalPages { get; set; }
}
public class UserListItem
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfilePic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int PostCount { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}