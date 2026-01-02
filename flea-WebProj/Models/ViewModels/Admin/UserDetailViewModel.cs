using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels.Shared;

namespace flea_WebProj.Models.ViewModels.Admin;

public class UserDetailViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProfilePic { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }
    
    //Info de contacto
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TelegramUser { get; set; }
    
    //Direccion 
    public string? City { get; set; }
    public string StateProvince { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    //Posts del usuario
    public List<UserPostSummary> RecentPosts { get; set; } = [];
    public int TotalPosts { get; set; }
    
    //Roles del usuario
    
    public List<Role> Roles { get; set; } = [];
}