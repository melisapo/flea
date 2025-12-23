namespace flea_WebProj.Models;

public class Post
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long ProductId { get; set; }
    public long AuthorId { get; set; }
    
    public Product? Product { get; set; }
    public User? Author { get; set; }
}