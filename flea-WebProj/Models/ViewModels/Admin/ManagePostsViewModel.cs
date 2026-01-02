using flea_WebProj.Models.Enums;

namespace flea_WebProj.Models.ViewModels.Admin;

public class ManagePostsViewModel
{
    public List<ManagePostItemViewModel> Posts { get; set; } = new();

    // Filtros
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public ProductStatus? Status { get; set; }

    // Paginación
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalPosts { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalPosts / PageSize);
}