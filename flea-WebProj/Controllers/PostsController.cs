using flea_WebProj.Helpers;
using flea_WebProj.Middleware;
using flea_WebProj.Models.ViewModels.Product;
using flea_WebProj.Services;
using Microsoft.AspNetCore.Mvc;

namespace flea_WebProj.Controllers;

public class PostsController(
    IPostService postService,
    ICategoryService categoryService)
    : Controller
{
    // ==================== VER DETALLE DE POST ====================
    
    // GET: /Post/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = HttpContext.Session.GetUserId();
        var model = await postService.GetPostDetailAsync(id, userId);
        
        if (model == null)
            return NotFound();

        return View(model);
    }

    // ==================== CREAR POST ====================
    
    // GET: /Post/Create
    [HttpGet]
    [RequireAuth]
    public async Task<IActionResult> Create()
    {
        var model = new CreatePostViewModel
        {
            AvailableCategories = await categoryService.GetAllCategoriesAsync() ?? []
        };
        
        return View(model);
    }

    // POST: /Post/Create
    [HttpPost]
    [RequireAuth]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = await categoryService.GetAllCategoriesAsync() ?? [];
            return View(model);
        }

        var userId = HttpContext.Session.GetUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var (success, message, postId) = await postService.CreatePostAsync(model, userId.Value);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("Details", new { id = postId });
        }

        ModelState.AddModelError("", message);
        model.AvailableCategories = await categoryService.GetAllCategoriesAsync() ?? [];
        return View(model);
    }

    // ==================== EDITAR POST ====================
    
    // GET: /Post/Edit/5
    [HttpGet]
    [RequireAuth]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = HttpContext.Session.GetUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var model = await postService.GetEditPostDataAsync(id, userId.Value);
        
        if (model == null)
            return RedirectToAction("AccessDenied", "Account");

        return View(model);
    }

    // POST: /Post/Edit/5
    [HttpPost]
    [RequireAuth]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = await categoryService.GetAllCategoriesAsync() ?? [];
            return View(model);
        }

        var userId = HttpContext.Session.GetUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var (success, message) = await postService.UpdatePostAsync(model, userId.Value);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("Details", new { id = model.PostId });
        }
        
        model.AvailableCategories = await categoryService.GetAllCategoriesAsync() ?? [];
        ModelState.AddModelError("", message);
        return View(model);
    }

    // ==================== ELIMINAR POST ====================
    
    // POST: /Post/Delete/5
    [HttpPost]
    [RequireAuth]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var (success, message) = await postService.DeletePostAsync(id, userId.Value);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("MyPosts");
        }

        TempData["ErrorMessage"] = message;
        return RedirectToAction("Details", new { id });
    }

    // ==================== MIS PUBLICACIONES ====================
    
    // GET: /Post/MyPosts
    [HttpGet]
    [RequireAuth]
    public async Task<IActionResult> MyPosts()
    {
        var userId = HttpContext.Session.GetUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var model = await postService.GetUserPostsAsync(userId.Value);
        
        return View(model);
    }
}