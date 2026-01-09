using flea_WebProj.Helpers;
using flea_WebProj.Models.ViewModels.Auth;
using flea_WebProj.Models.ViewModels.Product;
using flea_WebProj.Models.ViewModels.Shared;
using flea_WebProj.Services;
using Microsoft.AspNetCore.Mvc;

namespace flea_WebProj.Controllers;

public class UserController(
    IAuthService authService,
    IPostService postService) : Controller
{
    // GET: /User/Profile/5 o /User/Profile?username=melissa
    public async Task<IActionResult> Profile(int? id = null, string? username = null)
    {
        // Buscar por ID o username
        var user = id.HasValue 
            ? await authService.GetFullUserProfileAsync(id.Value)
            : username != null 
                ? await authService.GetByUsernameAsync(username)
                : null;

        if (user == null)
            return RedirectToAction("Index", "Home");

        var sessionUserId = HttpContext.Session.GetUserId();

        if (sessionUserId != null && user.Id == sessionUserId)
        {
            return RedirectToAction("Profile", "Account");
        }

        // Obtener posts del usuario
        var userPosts = await postService.GetUserPostsAsync(user.Id, 12);

        var model = new UserProfileViewModel
        {
            UserId = user.Id,
            Username = user.Username,
            Name = user.Name,
            ProfilePic = user.ProfilePicture,
            MemberSince = user.CreatedAt,
            
            Email = user.Contact?.Email ?? "",
            PhoneNumber = user.Contact?.PhoneNumber,
            TelegramUser = user.Contact?.TelegramUser,
            
            City = user.Address?.City,
            StateProvince = user.Address?.StateProvince ?? "",
            Country = user.Address?.Country ?? "",
            
            RecentPosts = userPosts.Select<PostCardViewModel, UserPostSummary>(p => new UserPostSummary
            {
                PostId = p.PostId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                MainImage = p.MainImage,
                CreatedAt = p.CreatedAt
            }).ToList(),
            
            TotalPosts = userPosts.Count
        };

        return View(model);
    }
}
