namespace flea_WebProj.Models.ViewModels.Admin;

public class UserItemViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string RoleName { get; set; } = string.Empty;
    
}