using flea_WebProj.Models.Entities;
using flea_WebProj.Models.Enums;

namespace flea_WebProj.Models.ViewModels.Admin;

public class ManagePostsViewModel
{
    public List<PostManageItem> Posts { get; set; } = [];
    
    // Paginación
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPosts { get; set; }
    public int TotalPages { get; set; }
    
    // Para filtros
    public List<Category> AvailableCategories { get; set; } = [];
}

public class PostManageItem
{
    public int PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? MainImage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Reported { get; set; } = string.Empty;
    
    // Info del autor
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    
    // Categorías
    public List<string> Categories { get; set; } = [];
}