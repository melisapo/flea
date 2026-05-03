using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels.Product;

namespace flea_WebProj.Models.ViewModels;

public class HomeViewModel
{
    public List<PostCardViewModel> RecentPosts { get; set; } = [];
    public List<Category> PopularCategories { get; set; } = [];
    
    // Para paginación
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalPosts { get; set; } = 0;
    
    // Para filtros
    public int? SelectedCategoryId { get; set; }
    public string? SearchQuery { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}