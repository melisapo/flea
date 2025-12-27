namespace flea_WebProj.Models.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    
    public List<Product> Products { get; set; } = [];
}