using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels.Admin;

public class ManageCategoriesViewModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostCount { get; set; }
}

public class CreateCategoryViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El slug es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El slug debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones")]
    public string Slug { get; set; } = string.Empty;
}

public class EditCategoryViewModel
{
    public int CategoryId { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El slug es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El slug debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones")]
    public string Slug { get; set; } = string.Empty;
    
    public int PostCount { get; set; }
}