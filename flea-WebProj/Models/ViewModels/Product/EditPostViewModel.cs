using System.ComponentModel.DataAnnotations;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.Enums;
using flea_WebProj.Models.ViewModels.Shared;

namespace flea_WebProj.Models.ViewModels.Product;

public class EditPostViewModel
{
    public int PostId { get; set; }
    public int ProductId { get; set; }

    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(255, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 255 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 1000 caracteres")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "El estado es requerido")]
    public ProductStatus Status { get; set; }
    
    public List<Category> AvailableCategories { get; set; } = [];
    
    [Required(ErrorMessage = "Debe seleccionar al menos una categoría")]
    public List<int> PostCategoriesIds { get; set; } = [];
    public List<ImageViewModel> ExistingImages { get; set; } = [];
    
    public List<IFormFile>? NewImages { get; set; }
    
    public List<int> ImagesToDelete { get; set; } = [];
    
}