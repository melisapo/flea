using flea_WebProj.Enums;

namespace flea_WebProj.Models.ViewModels;

public class SearchPostViewModel
{
    public string? SearchTerm { get; set; }
    public long? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ProductStatus? Status { get; set; }
    public string SortBy { get; set; } = "recent"; // "recent", "price-asc", "price-desc"
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    public List<PostCardViewModel> Results { get; set; } = [];
    public int TotalResults { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    
    public List<Category> AvailableCategories { get; set; } = [];
}