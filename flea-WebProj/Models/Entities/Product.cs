namespace flea_WebProj.Models.Entities;

public class Product
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;

    public List<Category> Categories { get; set; } = [];
    public List<Image> Images { get; set; } = [];
    public Post? Post { get; set; }
}
