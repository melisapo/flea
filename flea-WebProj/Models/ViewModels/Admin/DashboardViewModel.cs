namespace flea_WebProj.Models.ViewModels.Admin;

public class DashboardViewModel
{
    // Estadísticas generales
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int TotalCategories { get; set; }
    public int ActivePosts { get; set; }
    public int SoldPosts { get; set; }
    
    // Estadísticas recientes
    public int NewUsersThisWeek { get; set; }
    public int NewPostsThisWeek { get; set; }
    public int NewPostsToday { get; set; }
    
    // Actividad reciente
    public List<RecentActivityItem> RecentActivity { get; set; } = [];
    
    // Top categorías
    public List<CategoryStatsItem> TopCategories { get; set; } = [];
}

public class RecentActivityItem
{
    public string Type { get; set; } = string.Empty; // "user", "post", "category"
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty; // CSS class
}

public class CategoryStatsItem
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int PostCount { get; set; }
}