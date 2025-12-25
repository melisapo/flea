using System.Diagnostics;
using flea_WebProj.Helpers;
using Microsoft.AspNetCore.Mvc;
using flea_WebProj.Models.ViewModels;

namespace flea_WebProj.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.IsAuthenticated = HttpContext.Session.IsAuthenticated();

        if (!HttpContext.Session.IsAuthenticated()) return View();
        
        ViewBag.Username = HttpContext.Session.GetUsername();
        ViewBag.UserName = HttpContext.Session.GetUserName();
        ViewBag.IsAdmin = HttpContext.Session.IsAdmin();
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}