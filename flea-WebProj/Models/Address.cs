namespace flea_WebProj.Models;

public class Address
{
    public int Id { get; set; }
    public string? City { get; set; }
    public string StateProvince { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int UserId { get; set; }
    
    public User? User { get; set; }
}