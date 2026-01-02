using flea_WebProj.Models.Entities;

namespace flea_WebProj.Models.ViewModels.Admin;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public List<Post>? PostsToday { get; set; }
    public int TotalCategories { get; set; }
    public List<Category>? TrendingCategories { get; set; }
    public List<Address>? FreccuentAddresses { get; set; }
}