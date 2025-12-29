using System.Diagnostics;
using flea_WebProj.Helpers;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels;
using flea_WebProj.Models.ViewModels.Product;
using flea_WebProj.Services;
using Microsoft.AspNetCore.Mvc;

namespace flea_WebProj.Controllers;

public class HomeController(IPostService postService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null,
        int page = 1
    )
    {
        var searchModel = new SearchPostViewModel
        {
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Status = string.IsNullOrEmpty(status)
                ? null
                : Enum.Parse<Models.Enums.ProductStatus>(status),
            Page = page,
            PageSize = 12,
            Results = [], // ← INICIALIZAR
            AvailableCategories = [] // ← INICIALIZAR
        };
        
        try
        {
            searchModel.AvailableCategories = await postService.GetAllCategories();
            
            // Obtener posts (con filtros o todos)
            if (
                !string.IsNullOrEmpty(searchTerm)
                || categoryId.HasValue
                || minPrice.HasValue
                || maxPrice.HasValue
                || !string.IsNullOrEmpty(status)
            )
            {
                // Búsqueda con filtros
                searchModel.Results = await postService.SearchPostsAsync(searchModel);

                // Calcular total y páginas
                searchModel.TotalResults = await postService.GetTotalCountAsync(
                    searchModel.SearchTerm,
                    searchModel.CategoryId,
                    searchModel.MinPrice,
                    searchModel.MaxPrice,
                    searchModel.Status?.ToString()
                );
            }
            else
            {
                // Sin filtros, mostrar posts recientes
                var allPosts = await postService.GetRecentPostsAsync(100);
                searchModel.TotalResults = allPosts.Count;

                // Paginar manualmente
                searchModel.Results = allPosts
                    .Skip((page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .ToList();
            }

            // Calcular páginas
            searchModel.TotalPages = (int)
                Math.Ceiling(searchModel.TotalResults / (double)searchModel.PageSize);

        }
        catch (Exception ex)
        {
            // Si falla, al menos retornar modelo vacío
            Console.WriteLine($"Error en Index: {ex.Message}");
        }
        
        
        //Session
        ViewBag.IsAuthenticated = HttpContext.Session.IsAuthenticated();

        if (!HttpContext.Session.IsAuthenticated())
            return View();

        ViewBag.Username = HttpContext.Session.GetUsername();
        ViewBag.UserName = HttpContext.Session.GetUserName();
        ViewBag.IsAdmin = HttpContext.Session.IsAdmin();

        return View(searchModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
