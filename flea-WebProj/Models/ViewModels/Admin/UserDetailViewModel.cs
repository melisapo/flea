using flea_WebProj.Models.Enums;

namespace flea_WebProj.Models.ViewModels.Admin;

public class UserDetailViewModel
{
    // Info del usuario
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TelegramUser { get; set; }
    public string ProfilePic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Roles
    public List<string> Roles { get; set; } = new List<string>();
    
    // Ubicación
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? Country { get; set; }
    
    // Estadísticas
    public int TotalPosts { get; set; }
    public int ActivePosts { get; set; }
    public int SoldPosts { get; set; }
    
    // Posts del usuario
    public List<UserPostItem> RecentPosts { get; set; } = [];
}
public class UserPostItem
{
    public int PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductStatus Status { get; set; }
    public string MainImage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}