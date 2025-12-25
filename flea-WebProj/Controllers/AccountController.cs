using Microsoft.AspNetCore.Mvc;
using flea_WebProj.Services;
using flea_WebProj.Models.ViewModels.Account;
using flea_WebProj.Helpers;

namespace flea_WebProj.Controllers
{
    public class AccountController(IAuthService authService) : Controller
    {
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) 
                return View(model);

            var (success, message, user) = await authService.RegisterAsync(model);

            if (success && user != null)
            {
                // Guardar sesión
                HttpContext.Session.SetUser(user);

                TempData["SuccessMessage"] = "¡Registro exitoso! Bienvenido a Flea.";
                return RedirectToAction("Index", "Home");
            }
            
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }
        
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View(model);

            var (success, message, user) = await authService.LoginAsync(model);

            if (success && user != null)
            {
                // Guardar sesión
                HttpContext.Session.SetUser(user);

                TempData["SuccessMessage"] = $"¡Bienvenido de vuelta, {user.Name}!";
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.ClearUser();
            TempData["SuccessMessage"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult LogoutConfirm()
        {
            HttpContext.Session.ClearUser();
            TempData["SuccessMessage"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");

            var user = await authService.GetUserWithRolesAsync(userId.Value);
            if (user != null) return View(user);
            
            HttpContext.Session.ClearUser();
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}