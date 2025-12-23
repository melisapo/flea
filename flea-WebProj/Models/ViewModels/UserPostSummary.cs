namespace flea_WebProj.Models.ViewModels;

public class UserPostSummary
{
    public long PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string MainImage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}