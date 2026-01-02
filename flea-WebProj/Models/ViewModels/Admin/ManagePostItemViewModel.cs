using flea_WebProj.Models.Enums;

namespace flea_WebProj.Models.ViewModels.Admin;

public class ManagePostItemViewModel
{
    public long PostId { get; set; }
    public long ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string MainImage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorProfilePic { get; set; } = string.Empty;
}