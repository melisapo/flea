using flea_WebProj.Helpers;
using flea_WebProj.Middleware;
using flea_WebProj.Models.ViewModels.Auth;
using flea_WebProj.Services;
using Microsoft.AspNetCore.Mvc;

namespace flea_WebProj.Controllers
{
    public class AccountController(
        IAuthService authService,
        IUserService userService) : Controller
    {
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Home");

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
        [RequireAuth]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");

            var user = await authService.GetFullUserProfileAsync(userId.Value);

            if (user != null) return View(user);
            
            HttpContext.Session.ClearUser();
            return RedirectToAction("Login");

        }
        
        // GET: /Account/EditProfile 
        [HttpGet]
        [RequireAuth]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");

            var model = await userService.GetEditProfileDataAsync(userId.Value);
    
            if (model == null)
                return RedirectToAction("Login");

            return View(model);
        }

        // PUT: /Account/EditProfile
        [HttpPost]
        [RequireAuth]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");
            if (model.RemoveProfilePic)
            {
                model.CurrentProfilePic = "/images/default-avatar.png";
                var updatedUser = await authService.GetUserWithRolesAsync(userId.Value);
                if (updatedUser != null) 
                    HttpContext.Session.SetUser(updatedUser);
            }
            // Actualizar foto de perfil si se subió una nueva
            if (model.NewProfilePic != null)
            {
                var (success, _, newPath) = await userService.UpdateProfilePictureAsync(userId.Value, model.NewProfilePic);
        
                if (success && newPath != null)
                {
                    model.CurrentProfilePic = newPath;
                    // Actualizar sesión con nueva foto
                    var updatedUser = await authService.GetUserWithRolesAsync(userId.Value);
                    if (updatedUser != null) 
                        HttpContext.Session.SetUser(updatedUser);
                }
            }

            // Actualizar resto del perfil
            var (profileSuccess, profileMessage) = await userService.UpdateProfileAsync(userId.Value, model);

            if (profileSuccess)
            {
                TempData["SuccessMessage"] = "Perfil actualizado exitosamente";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", profileMessage);
            return View(model);
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        [RequireAuth]
        public IActionResult ChangePassword()
        {
            return View();
        }
        
        // PUT: /Account/ChangePassword
        [HttpPut]
        [RequireAuth]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login");
            
            var (success, message) = await userService.ChangePasswordAsync(userId.Value, model);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }
        
        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}