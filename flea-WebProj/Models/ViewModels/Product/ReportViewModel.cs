using System.ComponentModel.DataAnnotations;

namespace flea_WebProj.Models.ViewModels.Product;

public class ReportViewModel
{
    public int PostId { get; set; }
    public string? ReportMotive { get; set; }
}