using flea_WebProj.Data.Repositories;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.ViewModels.Product;
using flea_WebProj.Models.Enums;
using flea_WebProj.Helpers;
using flea_WebProj.Models.ViewModels.Shared;

namespace flea_WebProj.Services;

public interface IPostService
{
    Task<ProductDetailViewModel?> GetPostDetailAsync(int postId, int? currentUserId);
    Task<(bool success, string message, int postId)> CreatePostAsync(CreatePostViewModel model, int authorId);
    Task<(bool success, string message)> UpdatePostAsync(EditPostViewModel model, int userId);
    Task<(bool success, string message)> DeletePostAsync(int postId, int userId);
    Task<List<PostCardViewModel>> GetUserPostsAsync(int userId, int limit = 50);
    Task<EditPostViewModel?> GetEditPostDataAsync(int postId, int userId);
    Task<List<PostCardViewModel>> GetRecentPostsAsync(int limit = 12);
    Task<List<PostCardViewModel>> SearchPostsAsync(SearchPostsViewModel searchModel);
}

public class PostService(
    IPostRepository postRepository,
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IImageRepository imageRepository,
    IFileUploadService fileUploadService,
    IUserRepository userRepository,
    IContactRepository contactRepository)
    : IPostService
{
    // Obtener detalle completo de un post
    public async Task<ProductDetailViewModel?> GetPostDetailAsync(int postId, int? currentUserId)
    {
        var post = await postRepository.GetWithFullDetailsAsync(postId);
        
        if (post == null || post.Product == null || post.Author == null)
            return null;

        var isOwner = currentUserId.HasValue && currentUserId.Value == post.AuthorId;

        var model = new ProductDetailViewModel
        {
            PostId = post.Id,
            Title = post.Title,
            Description = post.Description,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            
            ProductId = post.Product.Id,
            Price = post.Product.Price,
            Status = post.Product.GetStatus(),
            StatusText = post.Product.GetStatusText(),
            
            Images = post.Product.Images.Select<Image, ImageViewModel>(i => new ImageViewModel
            {
                Id = i.Id,
                Path = i.Path,
                FullUrl = i.Path
            }).ToList(),
            
            Categories = post.Product.Categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug
            }).ToList(),
            
            AuthorId = post.Author.Id,
            AuthorUsername = post.Author.Username,
            AuthorName = post.Author.Name,
            AuthorProfilePic = post.Author.ProfilePicture,
            
            IsOwner = isOwner
        };

        // Obtener contacto del autor
        var contact = await contactRepository.GetByUserIdAsync(post.AuthorId);
        if (contact == null) return model;
        
        model.AuthorEmail = contact.Email;
        model.AuthorPhoneNumber = contact.PhoneNumber;
        model.AuthorTelegramUser = contact.TelegramUser;

        return model;
    }

    // Crear post
    public async Task<(bool success, string message, int postId)> CreatePostAsync(CreatePostViewModel model, int authorId)
    {
        try
        {
            // 1. Crear producto
            var product = new Product
            {
                Price = model.Price,
                Status = ProductStatus.Available.ToStatusString()
            };
            var productId = await productRepository.CreateAsync(product);

            // 2. Crear post
            var post = new Post
            {
                Title = model.Title,
                Description = model.Description,
                ProductId = productId,
                AuthorId = authorId
            };
            var postId = await postRepository.CreateAsync(post);

            // 3. Subir imágenes
            if (model.Images.Count != 0)
            {
                var uploadResults = await fileUploadService.UploadMultipleImagesAsync(
                    model.Images.ToList()
                );

                foreach (var (success, filePath, _) in uploadResults)
                {
                    if (success && filePath != null)
                    {
                        await imageRepository.CreateAsync(new Image
                        {
                            Path = filePath,
                            ProductId = productId
                        });
                    }
                }
            }

            // 4. Asignar categorías
            if (model.CategoryIds.Count == 0) return (false, "Error al crear publicación", 0);
            
            foreach (var categoryId in model.CategoryIds)
                await categoryRepository.AssignCategoryToProductAsync(productId, categoryId);

            return (true, "Publicación creada exitosamente", postId);
        }
        catch (Exception ex)
        {
            return (false, $"Error al crear publicación: {ex.Message}", 0);
        }
    }

    // Obtener datos para editar post
    public async Task<EditPostViewModel?> GetEditPostDataAsync(int postId, int userId)
    {
        var post = await postRepository.GetWithFullDetailsAsync(postId);
        
        if (post?.Product == null)
            return null;

        // Verificar que el usuario sea el dueño
        if (post.AuthorId != userId)
            return null;

        return new EditPostViewModel
        {
            PostId = post.Id,
            ProductId = post.Product.Id,
            Title = post.Title,
            Description = post.Description,
            Price = post.Product.Price,
            Status = post.Product.GetStatus(),
            CategoryIds = post.Product.Categories.Select(c => c.Id).ToList(),
            ExistingImages = post.Product.Images.Select(i => new ImageViewModel
            {
                Id = i.Id,
                Path = i.Path,
                FullUrl = i.Path
            }).ToList(),
            AvailableCategories = await categoryRepository.GetAllAsync()
        };
    }

    // Actualizar post
    public async Task<(bool success, string message)> UpdatePostAsync(EditPostViewModel model, int userId)
    {
        try
        {
            // 1. Verificar que el post existe y el usuario es el dueño
            var post = await postRepository.GetByIdAsync(model.PostId);
            if (post == null)
                return (false, "Publicación no encontrada");

            if (post.AuthorId != userId)
                return (false, "No tienes permiso para editar esta publicación");

            // 2. Actualizar post
            post.Title = model.Title;
            post.Description = model.Description;
            await postRepository.UpdateAsync(post);

            // 3. Actualizar producto
            var product = await productRepository.GetByIdAsync(model.ProductId);
            if (product != null)
            {
                product.Price = model.Price;
                product.Status = model.Status.ToStatusString();
                await productRepository.UpdateAsync(product);
            }

            // 4. Eliminar imágenes seleccionadas
            if (model.ImagesToDelete.Count != 0)
            {
                foreach (var imageId in model.ImagesToDelete)
                {
                    var image = await imageRepository.GetByIdAsync(imageId);
                    if (image != null)
                    {
                        fileUploadService.DeleteImage(image.Path);
                        await imageRepository.DeleteAsync(imageId);
                    }
                }
            }

            // 5. Subir nuevas imágenes
            if (model.NewImages != null && model.NewImages.Count != 0)
            {
                var uploadResults = await fileUploadService.UploadMultipleImagesAsync(
                    model.NewImages.ToList()
                );

                foreach (var (success, filePath, _) in uploadResults)
                {
                    if (success && filePath != null)
                    {
                        await imageRepository.CreateAsync(new Image
                        {
                            Path = filePath,
                            ProductId = model.ProductId
                        });
                    }
                }
            }

            // 6. Actualizar categorías
            var currentCategories = await categoryRepository.GetProductCategoryAsync(model.ProductId);
            foreach (var category in currentCategories)
            {
                await categoryRepository.RemoveCategoryFromProductAsync(model.ProductId, category.Id);
            }

            if (model.CategoryIds.Count == 0) return (false, "Error al actualizar publicación");
            
            foreach (var categoryId in model.CategoryIds)
            {
                await categoryRepository.AssignCategoryToProductAsync(model.ProductId, categoryId);
            }

            return (true, "Publicación actualizada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar publicación: {ex.Message}");
        }
    }

    // Eliminar post
    public async Task<(bool success, string message)> DeletePostAsync(int postId, int userId)
    {
        try
        {
            var post = await postRepository.GetWithFullDetailsAsync(postId);
            
            if (post == null)
                return (false, "Publicación no encontrada");

            if (post.AuthorId != userId)
                return (false, "No tienes permiso para eliminar esta publicación");

            // 1. Eliminar imágenes del servidor
            if (post.Product?.Images != null)
            {
                foreach (var image in post.Product.Images)
                {
                    fileUploadService.DeleteImage(image.Path);
                    await imageRepository.DeleteAsync(image.Id);
                }
            }

            // 2. Eliminar relaciones de categorías
            if (post.Product != null)
            {
                var categories = await categoryRepository.GetProductCategoryAsync(post.Product.Id);
                foreach (var category in categories)
                {
                    await categoryRepository.RemoveCategoryFromProductAsync(post.Product.Id, category.Id);
                }
            }

            // 3. Eliminar post
            await postRepository.DeleteAsync(postId);

            // 4. Eliminar producto
            if (post.Product != null)
            {
                await productRepository.DeleteAsync(post.Product.Id);
            }

            return (true, "Publicación eliminada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar publicación: {ex.Message}");
        }
    }

    // Obtener posts del usuario
    public async Task<List<PostCardViewModel>> GetUserPostsAsync(int userId, int limit = 50)
    {
        var posts = await postRepository.GetByAuthorAsync(userId, limit);
        var model = new List<PostCardViewModel>();

        foreach (var post in posts)
        {
            var product = await productRepository.GetWithDetailsAsync(post.ProductId);
            var author = await userRepository.GetByIdAsync(post.AuthorId);
            
            if (product != null && author != null)
            {
                model.Add(new PostCardViewModel
                {
                    PostId = post.Id,
                    ProductId = product.Id,
                    Title = post.Title,
                    Description = post.Description.Length > 100 
                        ? string.Concat(post.Description.AsSpan(0, 100), "...")
                        : post.Description,
                    Price = product.Price,
                    Status = product.GetStatus(),
                    StatusText = product.GetStatusText(),
                    MainImage = product.Images.FirstOrDefault()?.Path ?? "/images/no-image.png",
                    CreatedAt = post.CreatedAt,
                    AuthorUsername = author.Username,
                    AuthorProfilePic = author.ProfilePicture
                });
            }
        }

        return model;
    }

    // Obtener posts recientes
    public async Task<List<PostCardViewModel>> GetRecentPostsAsync(int limit = 12)
    {
        var posts = await postRepository.GetRecentPostsAsync(limit);
        var model = new List<PostCardViewModel>();

        foreach (var post in posts)
        {
            var product = await productRepository.GetWithDetailsAsync(post.ProductId);
            var author = await userRepository.GetByIdAsync(post.AuthorId);
            
            if (product != null && author != null)
            {
                model.Add(new PostCardViewModel
                {
                    PostId = post.Id,
                    ProductId = product.Id,
                    Title = post.Title,
                    Description = post.Description.Length > 100 
                        ? string.Concat(post.Description.AsSpan(0, 100), "...")
                        : post.Description,
                    Price = product.Price,
                    Status = product.GetStatus(),
                    StatusText = product.GetStatusText(),
                    MainImage = product.Images.FirstOrDefault()?.Path ?? "/images/no-image.png",
                    CreatedAt = post.CreatedAt,
                    AuthorUsername = author.Username,
                    AuthorProfilePic = author.ProfilePicture
                });
            }
        }

        return model;
    }

    // Buscar posts con filtros
    public async Task<List<PostCardViewModel>> SearchPostsAsync(SearchPostsViewModel searchModel)
    {
        var posts = await postRepository.GetWithFiltersAsync(
            searchModel.SearchTerm,
            searchModel.CategoryId,
            searchModel.MinPrice,
            searchModel.MaxPrice,
            searchModel.Status?.ToStatusString(),
            searchModel.Page,
            searchModel.PageSize
        );

        var model = new List<PostCardViewModel>();

        foreach (var post in posts)
        {
            var product = await productRepository.GetWithDetailsAsync(post.ProductId);
            var author = await userRepository.GetByIdAsync(post.AuthorId);
            
            if (product != null && author != null)
            {
                model.Add(new PostCardViewModel
                {
                    PostId = post.Id,
                    ProductId = product.Id,
                    Title = post.Title,
                    Description = post.Description.Length > 100 
                        ? string.Concat(post.Description.AsSpan(0, 100), "...")
                        : post.Description,
                    Price = product.Price,
                    Status = product.GetStatus(),
                    StatusText = product.GetStatusText(),
                    MainImage = product.Images.FirstOrDefault()?.Path ?? "/images/no-image.png",
                    CreatedAt = post.CreatedAt,
                    AuthorUsername = author.Username,
                    AuthorProfilePic = author.ProfilePicture
                });
            }
        }

        return model;
    }
}