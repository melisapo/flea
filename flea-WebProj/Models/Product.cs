namespace flea_WebProj.Models;

public class Product
{
    public long Id { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
}