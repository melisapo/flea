using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels.Product;

public class CategoryViewModel
{
    [StringLength(255, MinimumLength = 5, ErrorMessage = "El título debe tener entre 4 y 25 caracteres")]
    [Required(ErrorMessage = "El nombre es requerido")]
    public string Name { get; set; }  = string.Empty;
    
    [StringLength(255, MinimumLength = 5, ErrorMessage = "El slug debe tener entre 4 y 10 caracteres")]
    [Required(ErrorMessage = "El slug es requerido")]
    public string Slug { get; set; } =  string.Empty;
}