namespace flea_WebProj.Models.Entities;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ProductId { get; set; }
    public int AuthorId { get; set; }
    
    public Product? Product { get; set; }
    public User? Author { get; set; }
    public List<Image> Images { get; set; } = [];
}