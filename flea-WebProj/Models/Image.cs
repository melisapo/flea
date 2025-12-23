namespace flea_WebProj.Models;

public class Image
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public int ProductId { get; set; }
    
    public Product? Product { get; set; }
}