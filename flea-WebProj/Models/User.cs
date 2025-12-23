namespace flea_WebProj.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string ProfilePicture { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<Role>? Roles { get; set; } = [];
    public List<Address>? Addresses { get; set; } = [];
    public Contact? Contact { get; set; }
}