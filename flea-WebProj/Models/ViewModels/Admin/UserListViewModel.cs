using flea_WebProj.Models.Entities;

namespace flea_WebProj.Models.ViewModels.Admin;

public class UserListViewModel
{
    public List<UserItemViewModel>? Users { get; set; } = [];
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    
    public int TotalUsers { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalUsers / PageSize);
    
    public string? SearchTerm { get; set; }
    public int? RoleId { get; set; }
}