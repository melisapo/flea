using flea_WebProj.Models.Enums;
using flea_WebProj.Models.ViewModels.Shared;

namespace flea_WebProj.Models.ViewModels.Product;

public class ProductDetailViewModel
{
    public int PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public ProductStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    
    public List<ImageViewModel> Images { get; set; } = [];
    
    public List<CategoryViewModel> Categories { get; set; } = [];

    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorProfilePic { get; set; } = string.Empty;
    
    public string AuthorState { get; set; } = string.Empty;
    
    public string AuthorCountry {get; set; } = string.Empty;

    public string AuthorEmail { get; set; } = string.Empty;
    public string? AuthorPhoneNumber { get; set; }
    public string? AuthorTelegramUser { get; set; }

    public bool IsOwner { get; set; }
}