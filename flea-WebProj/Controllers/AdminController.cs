using flea_WebProj.Helpers;
using flea_WebProj.Middleware;
using flea_WebProj.Models.ViewModels.Admin;
using flea_WebProj.Services;
using Microsoft.AspNetCore.Mvc;

namespace flea_WebProj.Controllers
{
    [RequireAuth]
    [RequireAdminOrModerator]
    public class AdminController(
        IAdminService adminService,
        IPostService postService,
        ICategoryService categoryService) : Controller
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
        [RequireAdmin]
        public async Task<IActionResult> ManageUserRoles(int userId)
        {
            try
            {
                var user = await adminService.GetUserByIdAsync(userId);

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
        [RequireAdmin]
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
        [RequireAdmin]
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
        [RequireAdmin]
        public async Task<IActionResult> DeleteUserConfirm(int id)
        {
            try
            {
                var user = await adminService.GetUserByIdAsync(id);

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

        [HttpDelete]
        [ValidateAntiForgeryToken]
        [RequireAdmin]
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
                    var product = await postService.GetProductByPostIdAsync(post.Id);
                    var categoryList = await categoryService.GetByProductIdAsync(product.Id);
                    
                        viewModel.Posts.Add(new PostManageItem
                        {
                            PostId = post.Id,
                            Title = post.Title,
                            Description = post.Description,
                            Price = product.Price,
                            Status = product.GetStatus(),
                            StatusText = product.GetStatusText(),
                            MainImage = product.Images.FirstOrDefault()?.Path,
                            CreatedAt = post.CreatedAt,
                            AuthorId = author.Id,
                            AuthorUsername = author.Username,
                            AuthorName = author.Name,
                            Categories = categoryList
                                .Select(c => c.Name)
                                .ToList()
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
        [RequireModerator]
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

        [HttpDelete]
        [RequireModerator]
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
        public async Task<IActionResult> Categories(int? editId = null)
        {
            try
            {
                var categories = await categoryService.GetAllCategoriesAsync() ?? [];
                    var viewModel = new ManageCategoriesViewModel
                    {
                        Categories = categories.Select(c => new CategoryManageItem
                        {
                            CategoryId = c.Id,
                            Name = c.Name,
                            Slug = c.Slug,
                            PostCount = c.Products.Count
                        }).ToList()
                    };
                
                    if (!editId.HasValue) return View(viewModel);
                    {
                        var category = categories.FirstOrDefault(c => c.Id == editId.Value);
                        if (category == null) return View(viewModel);
                    
                        viewModel.IsEditMode = true;
                        viewModel.Form.CategoryId = category.Id;
                        viewModel.Form.Name = category.Name;
                        viewModel.Form.Slug = category.Slug;
                    }

                    return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar categorías: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAdmin]
        public async Task<IActionResult> SaveCategory(CategoryFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los campos del formulario";
                return RedirectToAction(nameof(Categories), new { editId = form.CategoryId });
            }

            try
            {
                if (form.CategoryId is null)
                {
                    await categoryService.CreateCategoryAsync(form);
                    TempData["SuccessMessage"] = "Categoría creada correctamente";
                }
                else
                {
                    await categoryService.UpdateCategoryAsync(form);
                    TempData["SuccessMessage"] = "Categoría actualizada correctamente";
                }

                return RedirectToAction(nameof(Categories));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Categories), new { editId = form.CategoryId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Categories));
            }
        }

        
        [HttpGet]
        [RequireAdmin]
        public async Task<IActionResult> DeleteCategoryConfirm(int id)
        {
            try
            {
                var category = await categoryService.GetCategoryByIdAsync(id);
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
        [RequireAdmin]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await categoryService.DeleteCategoryAsync(id);
                if (result.success)
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