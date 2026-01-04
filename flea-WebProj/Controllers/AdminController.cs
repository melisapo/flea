using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using flea_WebProj.Services;
using flea_WebProj.Models.ViewModels.Admin;
using flea_WebProj.Middleware;

namespace flea_WebProj.Controllers
{
    [Authorize]
    [RequireAdmin]
    public class AdminController(
        IAdminService adminService,
        IPostService postService) : Controller
    {
        // ============ DASHBOARD ============

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var viewModel = await adminService.GetDashboardStatsAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el dashboard: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // ============ USER MANAGEMENT ============

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await adminService.GetAllUsersAsync();
                var viewModel = new UserListViewModel
                {
                    Users = []
                };

                foreach (var user in users)
                {
                    var roles = await adminService.GetUserRolesAsync(user.Id);
                    if (user is { Contact: not null, Posts: not null })
                        viewModel.Users.Add(new UserListItem
                        {
                            UserId = user.Id,
                            Username = user.Username,
                            Name = user.Name,
                            Email = user.Contact.Email,
                            ProfilePic = user.ProfilePicture,
                            CreatedAt = user.CreatedAt,
                            PostCount = user.Posts.Count,
                            Roles = roles.Select(r => r.Name).ToList()
                        });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar usuarios: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<RedirectToActionResult> UserDetails(int id)
        {
            try
            {
                var user = await adminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado";
                    return RedirectToAction("Users");
                }
                
                var roles = await adminService.GetUserRolesAsync(id);
                if (user is not { Contact: not null, Address: not null, Posts: not null })
                    return RedirectToAction("Users");
                var viewModel = new UserDetailViewModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Contact.Email,
                    PhoneNumber = user.Contact.PhoneNumber,
                    TelegramUser = user.Contact.TelegramUser,
                    ProfilePic = user.ProfilePicture,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.Select(r => r.Name).ToList(),
                    City = user.Address.City,
                    StateProvince = user.Address.StateProvince,
                    Country = user.Address.Country,
                    TotalPosts = user.Posts.Count,
                    RecentPosts = await postService.GetUserPostsAsync(id)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar detalles: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        [HttpGet]
        [RequireAdmin]
        public async Task<IActionResult> ManageUserRoles(int userId)
        {
            try
            {
                var user = await adminService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado";
                    return RedirectToAction("Users");
                }

                var allRoles = await adminService.GetAllRolesAsync();
                var userRoles = await adminService.GetUserRolesAsync(userId);

                var viewModel = new ManageUserRolesViewModel
                {
                    UserId = userId,
                    Username = user.Username,
                    AvailableRoles = allRoles.Select(r => new RoleItem
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        IsAssigned = userRoles.Any(ur => ur.Id == r.Id)
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar roles: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(int userId, int roleId)
        {
            try
            {
                var result = await adminService.AssignRoleToUserAsync(userId, roleId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Rol asignado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo asignar el rol";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("ManageUserRoles", new { userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(int userId, int roleId)
        {
            try
            {
                var result = await adminService.RemoveRoleFromUserAsync(userId, roleId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Rol removido correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo remover el rol";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("ManageUserRoles", new { userId });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUserConfirm(int id)
        {
            try
            {
                var user = await adminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado";
                    return RedirectToAction("Users");
                }

                var viewModel = new DeleteConfirmationViewModel
                {
                    ItemType = "usuario",
                    ItemId = id,
                    ItemName = user.Username,
                    WarningMessage = "Se eliminarán todos los posts, productos e imágenes asociados a este usuario.",
                    ReturnUrl = Url.Action("Users")!
                };

                return View("DeleteConfirmation", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await adminService.DeleteUserAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Usuario eliminado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el usuario";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Users");
        }

        // ============ POST MANAGEMENT ============

        [HttpGet]
        public async Task<IActionResult> Posts()
        {
            try
            {
                var posts = await adminService.GetAllPostsAsync();
                var viewModel = new ManagePostsViewModel
                {
                    Posts = []
                };

                foreach (var post in posts)
                {
                    var author = await adminService.GetUserByIdAsync(post.AuthorId);
                    viewModel.Posts.Add(new PostManageItem
                    {
                        PostId = post.Id,
                        Title = post.Title,
                        AuthorName = author?.Username ?? "Usuario desconocido",
                        CreatedAt = post.CreatedAt
                    });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar posts: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeletePostConfirm(int id)
        {
            try
            {
                var post = await adminService.GetPostByIdAsync(id);
                if (post == null)
                {
                    TempData["ErrorMessage"] = "Post no encontrado";
                    return RedirectToAction("Posts");
                }

                var viewModel = new DeleteConfirmationViewModel
                {
                    ItemType = "post",
                    ItemId = id,
                    ItemName = post.Title,
                    WarningMessage = "Se eliminarán el producto y todas las imágenes asociadas.",
                    ReturnUrl = Url.Action("Posts")!
                };

                return View("DeleteConfirmation", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Posts");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var result = await adminService.DeletePostAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Post eliminado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el post";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Posts");
        }

        // ============ CATEGORY MANAGEMENT ============

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            try
            {
                var categories = await adminService.GetAllCategoriesAsync();
                var viewModel = new ManageCategoriesViewModel
                {
                    Categories = categories.Select(c => new CategoryManageItem
                    {
                        CategoryId = c.Id,
                        Name = c.Name,
                        Slug = c.Slug
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar categorías: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await adminService.CreateCategoryAsync(model);
                TempData["SuccessMessage"] = "Categoría creada correctamente";
                return RedirectToAction("Categories");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            try
            {
                var category = await adminService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Categoría no encontrada";
                    return RedirectToAction("Categories");
                }

                var viewModel = new EditCategoryViewModel
                {
                    CategoryId = category.Id,
                    Name = category.Name,
                    Slug = category.Slug
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Categories");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(EditCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await adminService.UpdateCategoryAsync(model);
                TempData["SuccessMessage"] = "Categoría actualizada correctamente";
                return RedirectToAction("Categories");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategoryConfirm(int id)
        {
            try
            {
                var category = await adminService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Categoría no encontrada";
                    return RedirectToAction("Categories");
                }

                var viewModel = new DeleteConfirmationViewModel
                {
                    ItemType = "categoría",
                    ItemId = id,
                    ItemName = category.Name,
                    WarningMessage = "No se puede eliminar si tiene productos asociados.",
                    ReturnUrl = Url.Action("Categories")!
                };

                return View("DeleteConfirmation", viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Categories");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await adminService.DeleteCategoryAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Categoría eliminada correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar la categoría";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Categories");
        }
    }
}