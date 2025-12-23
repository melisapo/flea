namespace flea_WebProj.Models.Entities;

public class Contact
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? TelegramUsername { get; set; } = string.Empty;
    public int UserId { get; set; }
    
    public User? User { get; set; }
}