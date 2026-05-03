namespace flea_WebProj.Models.ViewModels.Admin;

public class DeleteConfirmationViewModel
{
    public string ItemType { get; set; } = string.Empty; // "usuario", "post", "categoría"
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}