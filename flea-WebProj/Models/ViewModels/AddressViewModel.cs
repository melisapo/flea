using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels;

public class AddressViewModel
{
    public int? AddressId { get; set; }
    public string? City { get; set; }

    [Required(ErrorMessage = "El estado/provincia es requerido")]
    public string StateProvince { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país es requerido")]
    public string Country { get; set; } = string.Empty;

    public bool IsMain { get; set; } = false;
}